using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Project.Application.DTOs.Job.DomainJob;
using Project.Application.Exceptions;
using Project.Application.Jobs;
using Project.Domain.Entities;
using Project.Domain.Jobs;
using Project.Persistence.Repositories;
using Project.Web.AndroidAppsProject.Jobs;
using Xunit;

namespace Project.ServerRetrieval.Tests;

[Collection("Server retrieval SQL Server")]
public sealed class NativeJobSqlTests(SqlServerFixture fixture)
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Missing_scheduler_tables_are_created_automatically_and_preserve_legacy_settings(bool schedules, bool executions)
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString, initialize: false);
        db.Context.Jobs.Add(new Job { JobName = JobCatalog.Domain, JobPeriodTime = 10, IsActive = false, JobConfig = "{}" });
        await db.Context.SaveChangesAsync();
        if (schedules) await db.Context.Database.ExecuteSqlRawAsync("DROP TABLE JobSchedules");
        if (executions) await db.Context.Database.ExecuteSqlRawAsync("DROP TABLE JobExecutions");

        var dashboard = await db.ReadAsync();
        Assert.Equal(JobCatalog.All.Count, dashboard.Jobs.Count);
        Assert.False(dashboard.NeedsReview);
        Assert.False(dashboard.Jobs.Single(job => job.Definition.Name == JobCatalog.Domain).Settings.Enabled);
        Assert.Equal("{}", await db.Context.Jobs.AsNoTracking().Where(job => job.JobName == JobCatalog.Domain).Select(job => job.JobConfig).SingleAsync());
        Assert.Equal(5, await db.Context.Database.SqlQueryRaw<int>("""
            SELECT COUNT(*) AS Value FROM sys.indexes
            WHERE object_id IN (OBJECT_ID(N'JobSchedules'), OBJECT_ID(N'JobExecutions')) AND is_primary_key = 0 AND name IS NOT NULL
            """).SingleAsync());
        Assert.Equal(0, await db.Context.Database.SqlQueryRaw<int>("SELECT CASE WHEN OBJECT_ID(N'__EFMigrationsHistory') IS NULL THEN 0 ELSE 1 END AS Value").SingleAsync());
        await db.SaveAsync(JobCatalog.DeleteLogs, false);
        await db.SaveAsync(JobCatalog.CheckZones, false);
        await db.SaveAsync(JobCatalog.IpConfig, true);
        await db.RequestAsync(JobCatalog.IpConfig);
        Assert.True(await db.Worker.ExecuteNextAsync(CancellationToken.None));
        Assert.Equal(JobCatalog.IpConfig, Assert.Single(db.Tracker.Claims).JobName);
        Assert.Equal(JobExecutionStatus.Succeeded, Assert.Single((await db.ReadAsync()).History).Status);
    }

    [Fact]
    public async Task Latest_disabled_legacy_record_wins_and_invalid_json_is_quarantined()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString, initialize: false);
        db.Context.Jobs.Add(new Job { JobName = JobCatalog.Domain, JobPeriodTime = 10, IsActive = true, JobConfig = "{}" });
        await db.Context.SaveChangesAsync();
        db.Context.Jobs.Add(new Job { JobName = JobCatalog.Domain, JobPeriodTime = 20, IsActive = false, JobConfig = "{\"IsActiveJob\":true}" });
        db.Context.Jobs.Add(new Job { JobName = JobCatalog.SubDomain, JobPeriodTime = 10, IsActive = true, JobConfig = "{broken" });
        await db.Context.SaveChangesAsync();
        var dashboard = await db.ReadAsync();
        Assert.False(dashboard.Jobs.Single(job => job.Definition.Name == JobCatalog.Domain).Schedule.Enabled);
        Assert.Equal(20, dashboard.Jobs.Single(job => job.Definition.Name == JobCatalog.Domain).Settings.MinuteStep);
        var invalid = dashboard.Jobs.Single(job => job.Definition.Name == JobCatalog.SubDomain);
        Assert.False(invalid.Schedule.Enabled);
        Assert.NotNull(invalid.Schedule.ValidationError);
        Assert.True(dashboard.Jobs.Single(job => job.Definition.Name == JobCatalog.DeleteLogs).Schedule.Enabled);
        await Assert.ThrowsAsync<BadRequestException>(() => db.RequestAsync(JobCatalog.Domain));
    }

    [Fact]
    public async Task Disable_is_persisted_and_preserves_the_previous_settings_after_provider_restart()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(JobCatalog.Domain, true, 12);
        await using (var scope = db.Services.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<IJobStore>().SaveAsync(JobCatalog.Domain,
                new CreateDomainJobDTO { IsActiveJob = false }, "test-admin", CancellationToken.None);
        await using var restarted = db.CreateProvider();
        await using var restartedScope = restarted.CreateAsyncScope();
        var dashboard = await restartedScope.ServiceProvider.GetRequiredService<IJobStore>().GetDashboardAsync(CancellationToken.None);
        var job = dashboard.Jobs.Single(job => job.Definition.Name == JobCatalog.Domain);
        Assert.False(job.Schedule.Enabled);
        Assert.Null(job.Schedule.NextRunUtc);
        Assert.Equal(12, job.Settings.MinuteStep);
        Assert.Equal(30, job.Settings.ExpireMinutes);
        Assert.Equal(3, await db.Context.Jobs.CountAsync(job => job.JobName == JobCatalog.Domain));
        Assert.Equal("test-admin", await db.Context.Jobs.OrderByDescending(job => job.Id).Where(job => job.JobName == JobCatalog.Domain).Select(job => job.CreatedBy).FirstAsync());
    }

    [Fact]
    public async Task Settings_can_be_edited_while_disabled_and_used_on_the_next_enable()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(JobCatalog.Domain, false, 17);
        var disabled = (await db.ReadAsync()).Jobs.Single(job => job.Definition.Name == JobCatalog.Domain);
        Assert.False(disabled.Settings.Enabled);
        Assert.Equal(17, disabled.Settings.MinuteStep);
        var input = disabled.Settings.ToLegacy();
        input.IsActiveJob = true;
        await using (var scope = db.Services.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<IJobStore>().SaveAsync(JobCatalog.Domain, input, "test-admin", CancellationToken.None);
        var enabled = (await db.ReadAsync()).Jobs.Single(job => job.Definition.Name == JobCatalog.Domain);
        Assert.True(enabled.Settings.Enabled);
        Assert.Equal(17, enabled.Settings.MinuteStep);
    }

    [Fact]
    public async Task A_reused_admin_scope_reads_fresh_state_and_disable_clears_a_newer_manual_request()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(JobCatalog.Domain, true);
        await using var admin = db.Services.CreateAsyncScope();
        var store = admin.ServiceProvider.GetRequiredService<IJobStore>();
        var snapshot = await store.GetDashboardAsync(CancellationToken.None);
        await db.RequestAsync(JobCatalog.Domain); // A different request updates the same schedule after the read.
        var input = snapshot.Jobs.Single(job => job.Definition.Name == JobCatalog.Domain).Settings.ToLegacy();
        input.IsActiveJob = false;
        await store.SaveAsync(JobCatalog.Domain, input, "admin", CancellationToken.None);
        var disabled = (await db.ReadAsync()).Jobs.Single(job => job.Definition.Name == JobCatalog.Domain);
        Assert.False(disabled.Schedule.RunRequested);
        Assert.Null(disabled.Schedule.RequestedUtc);
        await db.SaveAsync(JobCatalog.Domain, true);
        Assert.False(await db.Worker.ExecuteNextAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Manual_requests_survive_restart_and_repeated_requests_coalesce()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(JobCatalog.Domain, true, 12);
        for (var index = 0; index < 5; index++) await db.RequestAsync(JobCatalog.Domain);
        await using var restarted = db.CreateProvider();
        var worker = restarted.GetServices<IHostedService>().OfType<NativeJobWorker>().Single();
        Assert.True(await worker.ExecuteNextAsync(CancellationToken.None));
        Assert.False(await worker.ExecuteNextAsync(CancellationToken.None));
        var run = Assert.Single((await db.ReadAsync()).History);
        Assert.Equal("Manual", run.Trigger);
        Assert.Equal(JobExecutionStatus.Succeeded, run.Status);
        Assert.Equal(12, Assert.Single(db.Tracker.Claims).Settings.MinuteStep);
    }

    [Fact]
    public async Task Pending_manual_request_uses_updated_settings_and_older_requests_go_first()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(JobCatalog.Domain, true, 10);
        await db.SaveAsync(JobCatalog.IpConfig, true);
        await db.RequestAsync(JobCatalog.IpConfig);
        await db.RequestAsync(JobCatalog.Domain);
        await db.SaveAsync(JobCatalog.Domain, true, 15);
        Assert.True(await db.Worker.ExecuteNextAsync(CancellationToken.None));
        Assert.Equal(JobCatalog.IpConfig, Assert.Single(db.Tracker.Claims).JobName);
        Assert.True(await db.Worker.ExecuteNextAsync(CancellationToken.None));
        Assert.Equal(15, db.Tracker.Claims.Last().Settings.MinuteStep);
        Assert.False(await db.Worker.ExecuteNextAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Two_instances_and_different_mutating_jobs_share_one_sql_execution_lock()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(JobCatalog.Domain, true);
        await db.SaveAsync(JobCatalog.IpConfig, true);
        await db.RequestAsync(JobCatalog.Domain);
        await db.RequestAsync(JobCatalog.IpConfig);
        var entered = Completion();
        var release = Completion();
        db.Tracker.Execute = async (_, _) => { entered.TrySetResult(); await release.Task; };
        await using var other = db.CreateProvider();
        var secondWorker = other.GetServices<IHostedService>().OfType<NativeJobWorker>().Single();
        var first = db.Worker.ExecuteNextAsync(CancellationToken.None);
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.False(await secondWorker.ExecuteNextAsync(CancellationToken.None));
            Assert.Equal(1, db.Tracker.Active);
        }
        finally { release.TrySetResult(); }
        Assert.True(await first);
        Assert.True(await secondWorker.ExecuteNextAsync(CancellationToken.None));
        Assert.Equal(1, db.Tracker.MaximumActive);
        Assert.Equal(2, db.Tracker.Claims.Count);
        Assert.All((await db.ReadAsync()).History, run => Assert.Equal(JobExecutionStatus.Succeeded, run.Status));
    }

    [Fact]
    public async Task Missed_slots_coalesce_and_the_next_slot_is_persisted_in_sql_utc()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(JobCatalog.Domain, true);
        var missed = DateTime.UtcNow.AddDays(-2);
        await db.Context.JobSchedules.Where(job => job.JobName == JobCatalog.Domain)
            .ExecuteUpdateAsync(update => update.SetProperty(job => job.NextRunUtc, missed));
        Assert.True(await db.Worker.ExecuteNextAsync(CancellationToken.None));
        Assert.False(await db.Worker.ExecuteNextAsync(CancellationToken.None));
        var dashboard = await db.ReadAsync();
        var run = Assert.Single(dashboard.History);
        Assert.Equal("Scheduled", run.Trigger);
        Assert.Equal(missed, run.ScheduledForUtc);
        Assert.True(dashboard.Jobs.Single(job => job.Definition.Name == JobCatalog.Domain).Schedule.NextRunUtc > DateTime.UtcNow);
    }

    [Theory]
    [InlineData(JobCatalog.Domain)]
    [InlineData(JobCatalog.DeleteLogs)]
    [InlineData(JobCatalog.CheckZones)]
    public async Task Every_abandoned_execution_blocks_all_jobs_until_review_without_replaying_it(string name)
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(name, true);
        await db.RequestAsync(name);
        JobClaim? abandoned;
        await using (var lease = await db.Services.GetRequiredService<IJobExecutionLock>().TryAcquireAsync(CancellationToken.None))
        {
            Assert.NotNull(lease);
            await using var scope = db.Services.CreateAsyncScope();
            abandoned = await scope.ServiceProvider.GetRequiredService<IJobStore>().ClaimNextAsync("crashed-worker", CancellationToken.None);
            Assert.NotNull(abandoned);
        }
        Assert.False(await db.Worker.ExecuteNextAsync(CancellationToken.None));
        var recovered = Assert.Single((await db.ReadAsync()).History);
        Assert.Equal(JobExecutionStatus.Interrupted, recovered.Status);
        Assert.True(recovered.NeedsReview);
        Assert.Empty(db.Tracker.Claims);
        await Assert.ThrowsAsync<BadRequestException>(() => db.RequestAsync(name));
        await using (var scope = db.Services.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<IJobStore>().ReviewAsync(recovered.Id, "reviewer", CancellationToken.None);
        await db.RequestAsync(name);
        Assert.True(await db.Worker.ExecuteNextAsync(CancellationToken.None));
        var history = (await db.ReadAsync()).History;
        Assert.Equal(2, history.Count);
        Assert.NotEqual(abandoned!.ExecutionId, Assert.Single(db.Tracker.Claims).ExecutionId);
        Assert.NotNull(history.Single(run => run.Id == recovered.Id).ReviewedUtc);
        Assert.Equal("reviewer", history.Single(run => run.Id == recovered.Id).ReviewedBy);
    }

    [Fact]
    public async Task Failure_stores_a_safe_code_and_requires_review_instead_of_retrying_dns()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(JobCatalog.Domain, true);
        await db.RequestAsync(JobCatalog.Domain);
        db.Tracker.Execute = (_, _) => Task.FromException(new Exception("synthetic-private-provider-message"));
        Assert.True(await db.Worker.ExecuteNextAsync(CancellationToken.None));
        Assert.False(await db.Worker.ExecuteNextAsync(CancellationToken.None));
        var run = Assert.Single((await db.ReadAsync()).History);
        Assert.Equal(JobExecutionStatus.Failed, run.Status);
        Assert.Equal("execution_failed", run.ErrorCode);
        Assert.True(run.NeedsReview);
        Assert.DoesNotContain("synthetic-private-provider-message", run.SettingsJson);
        Assert.DoesNotContain("ApiKey", run.SettingsJson);
    }

    [Fact]
    public async Task Configuration_changes_cancel_an_active_execution_and_persist_the_disabled_state()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(JobCatalog.Domain, true);
        await db.RequestAsync(JobCatalog.Domain);
        var entered = Completion();
        db.Tracker.Execute = async (_, token) => { entered.SetResult(); await Task.Delay(Timeout.InfiniteTimeSpan, token); };
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var run = db.Worker.ExecuteNextAsync(deadline.Token);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await db.SaveAsync(JobCatalog.Domain, false);
        Assert.True(await run.WaitAsync(TimeSpan.FromSeconds(10)));
        var dashboard = await db.ReadAsync();
        Assert.Equal(JobExecutionStatus.Cancelled, Assert.Single(dashboard.History).Status);
        Assert.Equal("configuration_changed", Assert.Single(dashboard.History).ErrorCode);
        Assert.False(dashboard.Jobs.Single(job => job.Definition.Name == JobCatalog.Domain).Schedule.Enabled);
        Assert.True(dashboard.NeedsReview);
    }

    [Fact]
    public async Task Enabled_edits_keep_the_current_snapshot_and_apply_to_the_next_execution()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(JobCatalog.Domain, true, 10);
        await db.RequestAsync(JobCatalog.Domain);
        var entered = Completion();
        var release = Completion();
        db.Tracker.Execute = async (_, token) =>
        {
            entered.TrySetResult();
            await release.Task.WaitAsync(token);
        };
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var running = db.Worker.ExecuteNextAsync(deadline.Token);
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await db.SaveAsync(JobCatalog.Domain, true, 15);
            await Task.Delay(TimeSpan.FromSeconds(2.5), deadline.Token);
            Assert.False(running.IsCompleted);
            Assert.Equal(10, Assert.Single(db.Tracker.Claims).Settings.MinuteStep);
        }
        finally { release.TrySetResult(); }
        Assert.True(await running);
        Assert.False((await db.ReadAsync()).NeedsReview);
        await db.RequestAsync(JobCatalog.Domain);
        Assert.True(await db.Worker.ExecuteNextAsync(CancellationToken.None));
        Assert.Equal(new[] { 10, 15 }, db.Tracker.Claims.Select(claim => claim.Settings.MinuteStep));
    }

    [Fact]
    public async Task In_place_database_edits_are_detected_without_a_process_restart()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(JobCatalog.Domain, true);
        var current = await db.Context.Jobs.Where(job => job.JobName == JobCatalog.Domain).OrderByDescending(job => job.Id).FirstAsync();
        var previousHash = (await db.ReadAsync()).Jobs.Single(job => job.Definition.Name == JobCatalog.Domain).Schedule.ConfigurationHash;
        current.JobPeriodTime = 15;
        current.IsActive = false;
        await db.Context.SaveChangesAsync();
        var updated = (await db.ReadAsync()).Jobs.Single(job => job.Definition.Name == JobCatalog.Domain);
        Assert.Equal(current.Id, updated.Schedule.ConfigurationId);
        Assert.NotEqual(previousHash, updated.Schedule.ConfigurationHash);
        Assert.False(updated.Schedule.Enabled);
        Assert.Equal(15, updated.Settings.MinuteStep);
    }

    [Fact]
    public async Task Losing_the_sql_lock_session_cancels_and_quarantines_the_execution()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(JobCatalog.SubDomain, true);
        await db.RequestAsync(JobCatalog.SubDomain);
        var entered = Completion();
        db.Tracker.Execute = async (_, token) => { entered.SetResult(); await Task.Delay(Timeout.InfiniteTimeSpan, token); };
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var running = db.Worker.ExecuteNextAsync(deadline.Token);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await using (var control = new SqlConnection(db.ConnectionString))
        {
            await control.OpenAsync();
            await using var command = control.CreateCommand();
            command.CommandText = "SELECT DISTINCT request_session_id FROM sys.dm_tran_locks WHERE resource_type = 'APPLICATION' AND resource_database_id = DB_ID()";
            var sessionId = Convert.ToInt32(await command.ExecuteScalarAsync());
            Assert.True(sessionId > 0);
            command.CommandText = "KILL " + sessionId.ToString(System.Globalization.CultureInfo.InvariantCulture);
            await command.ExecuteNonQueryAsync();
        }
        Assert.True(await running.WaitAsync(TimeSpan.FromSeconds(10)));
        var run = Assert.Single((await db.ReadAsync()).History);
        Assert.Equal(JobExecutionStatus.Interrupted, run.Status);
        Assert.Equal("connection_lost", run.ErrorCode);
        Assert.True(run.NeedsReview);
        Assert.False(await db.Worker.ExecuteNextAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Registered_dotnet_hosted_worker_executes_a_persisted_request_and_stops_cleanly()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await db.SaveAsync(JobCatalog.IpConfig, true);
        await db.RequestAsync(JobCatalog.IpConfig);
        await db.Worker.StartAsync(CancellationToken.None);
        try
        {
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (!(await db.ReadAsync()).History.Any(run => run.Status == JobExecutionStatus.Succeeded))
                await Task.Delay(50, deadline.Token);
        }
        finally { await db.Worker.StopAsync(CancellationToken.None); }
        Assert.Equal(JobCatalog.IpConfig, Assert.Single(db.Tracker.Claims).JobName);
        Assert.Equal(0, db.Tracker.Active);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Additive_migration_upgrades_existing_tables_without_changing_legacy_job_records(bool useSqlScript)
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString, initialize: false);
        var legacy = new Job { JobName = JobCatalog.Domain, JobPeriodTime = 15, IsActive = false, JobConfig = "{\"historical\":true}" };
        db.Context.Jobs.Add(legacy);
        await db.Context.SaveChangesAsync();
        await db.Context.Database.ExecuteSqlRawAsync("DROP TABLE JobExecutions; DROP TABLE JobSchedules;");
        var history = db.Context.GetService<IHistoryRepository>();
        await db.Context.Database.ExecuteSqlRawAsync(history.GetCreateScript());
        var migrations = db.Context.Database.GetMigrations().ToArray();
        Assert.EndsWith("AddNativeJobScheduling", migrations[^1]);
        foreach (var migration in migrations[..^1])
            await db.Context.Database.ExecuteSqlRawAsync(history.GetInsertScript(new HistoryRow(migration, "10.0.12")));
        await using var scope = db.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IJobStore>();
        if (useSqlScript)
        {
            var script = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "MigrationScripts", "native-job-scheduling.sql"));
            var batches = System.Text.RegularExpressions.Regex.Split(script, @"^\s*GO\s*$",
                System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            for (var attempt = 0; attempt < 2; attempt++)
                foreach (var batch in batches.Where(batch => !string.IsNullOrWhiteSpace(batch)))
                    await db.Context.Database.ExecuteSqlRawAsync(batch);
        }
        else await db.Context.Database.MigrateAsync();
        Assert.Empty(await db.Context.Database.GetPendingMigrationsAsync());
        Assert.Empty(await db.Context.JobExecutions.ToListAsync());
        var preserved = await db.Context.Jobs.AsNoTracking().SingleAsync();
        Assert.Equal(legacy.Id, preserved.Id);
        Assert.Equal(legacy.JobConfig, preserved.JobConfig);
        Assert.False(preserved.IsActive);
        // The store can use the schema produced by either manual migration path.
        var dashboard = await store.GetDashboardAsync(CancellationToken.None);
        Assert.Equal(JobCatalog.All.Count, dashboard.Jobs.Count);
        Assert.False(dashboard.Jobs.Single(job => job.Definition.Name == JobCatalog.Domain).Settings.Enabled);
        await db.SaveAsync(JobCatalog.DeleteLogs, false);
        await db.SaveAsync(JobCatalog.CheckZones, false);
        await db.SaveAsync(JobCatalog.IpConfig, true);
        await db.RequestAsync(JobCatalog.IpConfig);
        Assert.True(await db.Worker.ExecuteNextAsync(CancellationToken.None));
        Assert.Equal(JobCatalog.IpConfig, Assert.Single(db.Tracker.Claims).JobName);
    }

    [Fact]
    public async Task Native_log_cleanup_deletes_multiple_batches_without_tracking_every_log()
    {
        await using var db = new RetrievalDatabase(fixture.ConnectionString);
        await db.SeedAsync();
        db.Context.ServerLogs.AddRange(Enumerable.Range(0, 2200).Select(_ => new ServerLog { ServerId = db.Available.Id, UserId = "synthetic" }));
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();
        await new ServerLogRepository(db.Context).DeleteAllAsync(CancellationToken.None);
        Assert.Equal(0, await db.Context.ServerLogs.CountAsync());
        Assert.Empty(db.Context.ChangeTracker.Entries<ServerLog>());
    }

    [Fact]
    public async Task Log_cleanup_preserves_logs_that_arrive_after_its_watermark()
    {
        await using var db = new RetrievalDatabase(fixture.ConnectionString);
        await db.SeedAsync();
        var arriving = new ServerLog { ServerId = db.Available.Id, UserId = "synthetic-arrival" };
        var interceptor = new AfterFirstDelete(async () =>
        {
            db.Context.ServerLogs.Add(arriving);
            await db.Context.SaveChangesAsync();
        });
        await using var cleanup = new Project.Persistence.ApplicationDbContext(
            new DbContextOptionsBuilder<Project.Persistence.ApplicationDbContext>()
                .UseSqlServer(db.Context.Database.GetConnectionString()).AddInterceptors(interceptor).Options);
        await new ServerLogRepository(cleanup).DeleteAllAsync(CancellationToken.None);
        Assert.Equal(arriving.Id, Assert.Single(await db.Context.ServerLogs.AsNoTracking().ToListAsync()).Id);
    }

    private sealed class AfterFirstDelete(Func<Task> insert) : DbCommandInterceptor
    {
        private bool inserted;
        public override async ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
            int result, CancellationToken cancellationToken = default)
        {
            if (!inserted && command.CommandText.TrimStart().StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
            {
                inserted = true;
                await insert();
            }
            return result;
        }
    }

    private static TaskCompletionSource Completion() => new(TaskCreationOptions.RunContinuationsAsynchronously);
}
