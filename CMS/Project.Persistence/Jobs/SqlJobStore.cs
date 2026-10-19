#nullable enable
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Project.Application.DTOs.Job.DomainJob;
using Project.Application.Exceptions;
using Project.Application.Jobs;
using Project.Domain.Entities;
using Project.Domain.Jobs;

namespace Project.Persistence.Jobs;

public sealed class SqlJobStore(ApplicationDbContext database) : IJobStore
{
    public Task<JobDashboard> GetDashboardAsync(CancellationToken cancellationToken) => WithConfigurationLockAsync(async now =>
    {
        var jobs = await SynchronizeAsync(now, cancellationToken);
        var schedules = database.JobSchedules.Local.ToDictionary(job => job.JobName);
        var history = await database.JobExecutions.AsNoTracking().OrderByDescending(run => run.StartedUtc).Take(50).ToListAsync(cancellationToken);
        var needsReview = await database.JobExecutions.AnyAsync(run => run.NeedsReview, cancellationToken);
        return new JobDashboard(JobCatalog.All.Select(definition => new JobOverview(definition,
            ReadOrDefault(jobs[definition.Name]), schedules[definition.Name])).ToList(), history, needsReview);
    }, cancellationToken);

    public Task SaveAsync(string name, CreateDomainJobDTO input, string actor, CancellationToken cancellationToken) => WithConfigurationLockAsync(async now =>
    {
        JobCatalog.Get(name);
        var jobs = await SynchronizeAsync(now, cancellationToken);
        // Disabling must work even when the old payload is malformed or disabled form fields are absent.
        var effective = input;
        if (!input.IsActiveJob)
        {
            effective = ReadOrDefault(jobs[name]).ToLegacy();
            effective.IsActiveJob = false;
            effective.JobPeriodTime = input.JobPeriodTime ?? effective.JobPeriodTime;
            effective.JobExpireMinuteTime = input.JobExpireMinuteTime ?? effective.JobExpireMinuteTime;
            effective.FailConnectionCount = input.FailConnectionCount ?? effective.FailConnectionCount;
            effective.FailConnectionPercent = input.FailConnectionPercent ?? effective.FailConnectionPercent;
        }
        var settings = JobSettings.FromInput(name, effective);
        var job = NewConfiguration(name, settings, actor);
        database.Jobs.Add(job);
        await database.SaveChangesAsync(cancellationToken);
        var configuredBy = actor.Length > 256 ? actor[..256] : actor;
        await database.Jobs.Where(saved => saved.Id == job.Id).ExecuteUpdateAsync(update => update
            .SetProperty(saved => saved.CreatedBy, configuredBy).SetProperty(saved => saved.UpdatedBy, configuredBy), cancellationToken);
        Apply(database.JobSchedules.Local.Single(schedule => schedule.JobName == name), job, now);
        return true;
    }, cancellationToken);

    public Task RequestRunAsync(string name, CancellationToken cancellationToken) => WithConfigurationLockAsync(async now =>
    {
        JobCatalog.Get(name);
        await SynchronizeAsync(now, cancellationToken);
        var schedule = database.JobSchedules.Local.Single(schedule => schedule.JobName == name);
        if (!schedule.Enabled) throw new BadRequestException("ابتدا تنظیمات معتبر جاب را فعال و ذخیره کنید.");
        if (await database.JobExecutions.AnyAsync(run => run.NeedsReview, cancellationToken))
            throw new BadRequestException("ابتدا اجرای نامطمئن قبلی را در پنل بررسی و تأیید کنید.");
        if (!schedule.RunRequested) schedule.RequestedUtc = now;
        schedule.RunRequested = true; // Repeated requests coalesce without moving an older request to the end.
        return true;
    }, cancellationToken);

    public Task<JobClaim?> ClaimNextAsync(string workerId, CancellationToken cancellationToken) => WithConfigurationLockAsync<JobClaim?>(async now =>
    {
        var jobs = await SynchronizeAsync(now, cancellationToken);
        // Acquiring the execution session lock proves that a previous Running record has lost its owner.
        var interrupted = await database.JobExecutions.Where(run => run.Status == JobExecutionStatus.Running).ToListAsync(cancellationToken);
        foreach (var run in interrupted)
        {
            run.Status = JobExecutionStatus.Interrupted;
            run.ErrorCode = "process_interrupted";
            run.CompletedUtc = now;
            // A lost session does not prove the old process has stopped. Quarantine even an idempotent
            // cleanup/read job, since its old task could still interfere with a different job.
            run.NeedsReview = true;
            var schedule = database.JobSchedules.Local.Single(job => job.JobName == run.JobName);
            schedule.LastStatus = run.Status;
            schedule.LastCompletedUtc = now;
        }
        await database.SaveChangesAsync(cancellationToken);
        if (await database.JobExecutions.AnyAsync(run => run.NeedsReview, cancellationToken)) return null;

        var due = database.JobSchedules.Local.Where(job => job.Enabled && (job.RunRequested || job.NextRunUtc <= now))
            .OrderBy(job => job.RunRequested ? job.RequestedUtc ?? now : job.NextRunUtc)
            .ThenBy(job => job.JobName, StringComparer.Ordinal).FirstOrDefault();
        if (due == null) return null;
        var execution = new JobExecution
        {
            Id = Guid.NewGuid(), JobName = due.JobName, ConfigurationId = due.ConfigurationId, WorkerId = workerId,
            SettingsJson = JobSettings.Read(jobs[due.JobName]).Serialize(),
            Trigger = due.RunRequested ? "Manual" : "Scheduled", ScheduledForUtc = due.RunRequested ? due.RequestedUtc ?? now : due.NextRunUtc!.Value, StartedUtc = now
        };
        database.JobExecutions.Add(execution);
        due.RunRequested = false;
        due.RequestedUtc = null;
        due.LastStartedUtc = now;
        due.LastStatus = JobExecutionStatus.Running;
        due.NextRunUtc = MinuteSchedule.Next(now, due.MinuteStep); // Coalesce downtime; never replay every missed rotation.
        return new JobClaim(execution.Id, execution.JobName, execution.ConfigurationId, JobSettings.Read(jobs[due.JobName]), due.ConfigurationHash);
    }, cancellationToken);

    public Task CompleteAsync(JobClaim claim, string status, string? errorCode, bool requiresReview, CancellationToken cancellationToken) => WithConfigurationLockAsync(async now =>
    {
        var run = await database.JobExecutions.SingleAsync(run => run.Id == claim.ExecutionId && run.ConfigurationId == claim.ConfigurationId, cancellationToken);
        if (run.Status != JobExecutionStatus.Running) return false; // A recovered execution cannot be overwritten by its former owner.
        run.Status = status;
        run.ErrorCode = errorCode;
        run.CompletedUtc = now;
        run.NeedsReview = requiresReview;
        var schedule = await database.JobSchedules.SingleAsync(job => job.JobName == claim.JobName, cancellationToken);
        schedule.LastCompletedUtc = now;
        schedule.LastStatus = status;
        if (status == JobExecutionStatus.Succeeded) schedule.SuccessfulExecutions++;
        return true;
    }, cancellationToken);

    public Task ReviewAsync(Guid executionId, string actor, CancellationToken cancellationToken) => WithConfigurationLockAsync(async now =>
    {
        var run = await database.JobExecutions.SingleOrDefaultAsync(run => run.Id == executionId && run.NeedsReview, cancellationToken)
            ?? throw new BadRequestException("اجرای نیازمند بررسی پیدا نشد.");
        run.NeedsReview = false;
        run.ReviewedUtc = now;
        run.ReviewedBy = actor.Length > 256 ? actor[..256] : actor;
        return true;
    }, cancellationToken);

    private async Task<Dictionary<string, Job>> SynchronizeAsync(DateTime now, CancellationToken cancellationToken)
    {
        var names = JobCatalog.All.Select(job => job.Name).ToArray();
        var latest = await database.Jobs.AsNoTracking().Where(job => names.Contains(job.JobName)
            && !database.Jobs.Any(newer => newer.JobName == job.JobName && newer.Id > job.Id)).ToDictionaryAsync(job => job.JobName, cancellationToken);
        foreach (var definition in JobCatalog.All.Where(job => !latest.ContainsKey(job.Name)))
        {
            var configuration = NewConfiguration(definition.Name, JobSettings.Default(definition.Name), "SYSTEM");
            database.Jobs.Add(configuration);
            latest.Add(definition.Name, configuration);
        }
        await database.SaveChangesAsync(cancellationToken);
        await database.JobSchedules.LoadAsync(cancellationToken);
        foreach (var definition in JobCatalog.All)
        {
            var schedule = database.JobSchedules.Local.SingleOrDefault(job => job.JobName == definition.Name);
            if (schedule == null)
            {
                schedule = new JobSchedule { JobName = definition.Name };
                database.JobSchedules.Add(schedule);
            }
            if (schedule.ConfigurationHash != JobSettings.Fingerprint(latest[definition.Name])) Apply(schedule, latest[definition.Name], now);
        }
        return latest;
    }

    private static void Apply(JobSchedule schedule, Job configuration, DateTime now)
    {
        schedule.ConfigurationId = configuration.Id;
        schedule.ConfigurationHash = JobSettings.Fingerprint(configuration);
        var requested = schedule.RunRequested;
        schedule.RunRequested = false;
        try
        {
            var settings = JobSettings.Read(configuration);
            schedule.Enabled = settings.Enabled;
            schedule.MinuteStep = settings.MinuteStep;
            schedule.NextRunUtc = settings.Enabled ? MinuteSchedule.Next(now, settings.MinuteStep) : null;
            schedule.ValidationError = null;
            schedule.RunRequested = requested && settings.Enabled;
            if (!schedule.RunRequested) schedule.RequestedUtc = null;
        }
        catch (Exception exception) when (exception is JsonException or BadRequestException or ArgumentException)
        {
            schedule.Enabled = false;
            schedule.NextRunUtc = null;
            schedule.ValidationError = "تنظیمات ذخیره‌شده معتبر نیست؛ جاب تا اصلاح تنظیمات اجرا نمی‌شود.";
            schedule.RequestedUtc = null;
        }
    }

    private static JobSettings ReadOrDefault(Job configuration)
    {
        try { return JobSettings.Read(configuration); }
        catch (Exception exception) when (exception is JsonException or BadRequestException or ArgumentException)
        { return JobSettings.Default(configuration.JobName) with { Enabled = false }; }
    }

    private static Job NewConfiguration(string name, JobSettings settings, string actor) => new()
    {
        JobName = name, JobPeriodTime = settings.MinuteStep, IsActive = settings.Enabled,
        JobConfig = settings.Serialize(), CreatedBy = actor, UpdatedBy = actor
    };

    private async Task<T> WithConfigurationLockAsync<T>(Func<DateTime, Task<T>> action, CancellationToken cancellationToken)
    {
        // A controller can read and then save through the same scoped store. Refresh only scheduler-owned
        // entities so another request's pending flag is not hidden by EF tracking; leave Identity/business tracking alone.
        foreach (var entry in database.ChangeTracker.Entries().Where(entry => entry.Entity is Job or JobSchedule or JobExecution).ToArray())
            entry.State = EntityState.Detached;
        // This short transaction also serializes configuration saves with a claim, across all instances.
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        // Check before touching legacy settings. Do not cache failures: the next poll/request can recover
        // after deployment applies the migration. Runtime credentials only need access, never DDL rights.
        var schemaReady = await database.Database.SqlQueryRaw<int>("""
            SELECT CASE WHEN OBJECT_ID(N'JobSchedules', N'U') IS NOT NULL
                         AND OBJECT_ID(N'JobExecutions', N'U') IS NOT NULL
                        THEN 1 ELSE 0 END AS Value
            """).SingleAsync(cancellationToken);
        if (schemaReady == 0) throw new JobSchemaNotReadyException();
        await database.Database.ExecuteSqlRawAsync("""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource = N'RazdanehCMS:jobs:configuration:v1',
                @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;
            IF @result < 0 THROW 51000, N'Job configuration lock unavailable', 1;
            """, cancellationToken);
        var now = await database.Database.SqlQueryRaw<DateTime>("SELECT SYSUTCDATETIME() AS Value").SingleAsync(cancellationToken);
        var result = await action(DateTime.SpecifyKind(now, DateTimeKind.Utc));
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }
}
