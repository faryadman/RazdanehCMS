using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Project.Application.Jobs;
using Project.Domain.Entities;
using Project.Domain.Jobs;
using Project.Persistence;
using Project.Persistence.Jobs;
using Xunit;

namespace Project.ServerRetrieval.Tests;

[Collection("Server retrieval SQL Server")]
public sealed class NativeJobSchemaSqlTests(SqlServerFixture fixture)
{
    [Fact]
    public async Task Concurrent_instances_create_tables_once_and_record_only_the_native_migration()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString, initialize: false);
        await DropTablesAsync(db, true, true);
        await CreateHistoryAsync(db, includeNative: false);
        await using var second = db.CreateProvider();
        await using var scope = second.CreateAsyncScope();
        var other = scope.ServiceProvider.GetRequiredService<IJobStore>();
        var dashboards = await Task.WhenAll(db.ReadAsync(), other.GetDashboardAsync(CancellationToken.None));
        Assert.All(dashboards, dashboard => Assert.Equal(JobCatalog.All.Count, dashboard.Jobs.Count));
        Assert.Equal(JobCatalog.All.Count, await db.Context.Jobs.CountAsync());
        Assert.Equal(JobCatalog.All.Count, await db.Context.JobSchedules.CountAsync());
        Assert.Empty(await db.Context.Database.GetPendingMigrationsAsync());
        Assert.Equal(db.Context.Database.GetMigrations().Count(), (await db.Context.Database.GetAppliedMigrationsAsync()).Count());
        await db.ReadAsync();
        Assert.Equal(JobCatalog.All.Count, await db.Context.Jobs.CountAsync());
        Assert.Empty(await db.Context.JobExecutions.ToListAsync());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Recreating_lost_state_preserves_surviving_rows_and_requires_review(bool schedules, bool executions)
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString);
        await CreateHistoryAsync(db, includeNative: true);
        await db.SaveAsync(JobCatalog.Domain, true);
        await db.RequestAsync(JobCatalog.Domain);
        var before = await db.Context.JobSchedules.AsNoTracking().SingleAsync(job => job.JobName == JobCatalog.Domain);
        var previous = new JobExecution
        {
            Id = Guid.NewGuid(), JobName = JobCatalog.Domain, ConfigurationId = before.ConfigurationId,
            SettingsJson = "{}", WorkerId = "synthetic-worker", Trigger = "Manual",
            StartedUtc = DateTime.UtcNow, ScheduledForUtc = DateTime.UtcNow,
            CompletedUtc = DateTime.UtcNow, Status = JobExecutionStatus.Succeeded
        };
        db.Context.JobExecutions.Add(previous);
        await db.Context.SaveChangesAsync();
        var configurations = await db.Context.Jobs.CountAsync();
        await DropTablesAsync(db, schedules, executions);

        var dashboard = await db.ReadAsync();
        Assert.True(dashboard.NeedsReview);
        var recovered = Assert.Single(dashboard.History, run => run.ErrorCode == "schema_recreated");
        Assert.True(recovered.NeedsReview);
        Assert.Equal(JobExecutionStatus.Interrupted, recovered.Status);
        if (!executions) Assert.Contains(dashboard.History, run => run.Id == previous.Id && run.Status == previous.Status);
        if (!schedules)
        {
            var preserved = dashboard.Jobs.Single(job => job.Definition.Name == JobCatalog.Domain).Schedule;
            Assert.Equal(before.ConfigurationId, preserved.ConfigurationId);
            Assert.Equal(before.NextRunUtc, preserved.NextRunUtc);
            Assert.Equal(before.RequestedUtc, preserved.RequestedUtc);
            Assert.True(preserved.RunRequested);
        }
        Assert.Equal(configurations, await db.Context.Jobs.CountAsync());
        Assert.False(await db.Worker.ExecuteNextAsync(CancellationToken.None));
        Assert.Empty(db.Tracker.Claims);
        await using var scope = db.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IJobStore>().ReviewAsync(recovered.Id, "test-admin", CancellationToken.None);
        Assert.False((await db.ReadAsync()).NeedsReview);
    }

    [Fact]
    public async Task Denied_ddl_rolls_back_without_changing_settings_and_recovers_after_a_grant()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString, initialize: false);
        db.Context.Jobs.Add(new Job { JobName = JobCatalog.Domain, JobPeriodTime = 10, IsActive = false, JobConfig = "{}" });
        await db.Context.SaveChangesAsync();
        await DropTablesAsync(db, true, true);
        await db.Context.Database.ExecuteSqlRawAsync("""
            CREATE USER job_schema_test WITHOUT LOGIN;
            GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO job_schema_test;
            """);
        await using var scope = db.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var store = scope.ServiceProvider.GetRequiredService<IJobStore>();
        await context.Database.OpenConnectionAsync();
        await context.Database.ExecuteSqlRawAsync("EXECUTE AS USER = 'job_schema_test'");
        try
        {
            var error = await Assert.ThrowsAsync<JobSchemaNotReadyException>(() => store.GetDashboardAsync(CancellationToken.None));
            Assert.Contains("CREATE TABLE", error.Message);
            Assert.Null(error.InnerException);
            Assert.Equal(1, await db.Context.Jobs.CountAsync());
            Assert.Equal(0, await TableCountAsync(db));
            await db.Context.Database.ExecuteSqlRawAsync("""
                GRANT CREATE TABLE TO job_schema_test;
                GRANT ALTER ON SCHEMA::dbo TO job_schema_test;
                """);
            Assert.Equal(JobCatalog.All.Count, (await store.GetDashboardAsync(CancellationToken.None)).Jobs.Count);
            Assert.Equal(2, await TableCountAsync(db));
            await db.Context.Database.ExecuteSqlRawAsync("""
                REVOKE CREATE TABLE TO job_schema_test;
                REVOKE ALTER ON SCHEMA::dbo TO job_schema_test;
                """);
            Assert.Equal(JobCatalog.All.Count, (await store.GetDashboardAsync(CancellationToken.None)).Jobs.Count);
        }
        finally { await context.Database.ExecuteSqlRawAsync("REVERT"); }
    }

    [Fact]
    public async Task Cancellation_during_second_table_creation_rolls_back_the_first_table_and_history()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString, initialize: false);
        await DropTablesAsync(db, true, true);
        await CreateHistoryAsync(db, includeNative: false);
        using var cancelled = new CancellationTokenSource();
        await using var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(db.ConnectionString).AddInterceptors(new CancelSecondTable(cancelled)).Options);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new SqlJobStore(context).GetDashboardAsync(cancelled.Token));
        Assert.Equal(0, await TableCountAsync(db));
        Assert.Empty(await db.Context.Jobs.ToListAsync());
        Assert.Equal(new[] { JobSchemaNotReadyException.MigrationId }, await db.Context.Database.GetPendingMigrationsAsync());
        Assert.Equal(JobCatalog.All.Count, (await db.ReadAsync()).Jobs.Count);
    }

    [Fact]
    public async Task Database_without_legacy_jobs_is_not_initialized_as_an_application_database()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString, initialize: false);
        await DropTablesAsync(db, true, true);
        await db.Context.Database.ExecuteSqlRawAsync("DROP TABLE Jobs");
        var error = await Assert.ThrowsAsync<JobSchemaNotReadyException>(() => db.ReadAsync());
        Assert.Contains("Jobs", error.Message);
        Assert.Equal(0, await TableCountAsync(db));
    }

    [Fact]
    public async Task Hosted_worker_creates_missing_tables_before_claiming_a_job()
    {
        await using var db = await JobTestDatabase.CreateAsync(fixture.ConnectionString, initialize: false);
        await DropTablesAsync(db, true, true);
        db.Context.Jobs.AddRange(JobCatalog.All.Select(definition =>
        {
            var settings = JobSettings.Default(definition.Name) with { Enabled = definition.Name == JobCatalog.IpConfig };
            return new Job { JobName = definition.Name, IsActive = settings.Enabled, JobPeriodTime = settings.MinuteStep, JobConfig = settings.Serialize() };
        }));
        await db.Context.SaveChangesAsync();
        await db.Worker.StartAsync(CancellationToken.None);
        try
        {
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            while (await TableCountAsync(db) != 2) await Task.Delay(50, deadline.Token);
            await db.RequestAsync(JobCatalog.IpConfig);
            while (Volatile.Read(ref db.Tracker.Active) != 0 || db.Tracker.Claims.IsEmpty) await Task.Delay(50, deadline.Token);
            Assert.Equal(JobCatalog.IpConfig, Assert.Single(db.Tracker.Claims).JobName);
        }
        finally { await db.Worker.StopAsync(CancellationToken.None); }
    }

    private static async Task CreateHistoryAsync(JobTestDatabase db, bool includeNative)
    {
        var history = db.Context.GetService<IHistoryRepository>();
        await db.Context.Database.ExecuteSqlRawAsync(history.GetCreateScript());
        foreach (var migration in db.Context.Database.GetMigrations().Where(id => includeNative || id != JobSchemaNotReadyException.MigrationId))
            await db.Context.Database.ExecuteSqlRawAsync(history.GetInsertScript(new HistoryRow(migration, "10.0.12")));
    }

    private static async Task DropTablesAsync(JobTestDatabase db, bool schedules, bool executions)
    {
        if (schedules) await db.Context.Database.ExecuteSqlRawAsync("DROP TABLE JobSchedules");
        if (executions) await db.Context.Database.ExecuteSqlRawAsync("DROP TABLE JobExecutions");
    }

    private static Task<int> TableCountAsync(JobTestDatabase db) => db.Context.Database.SqlQueryRaw<int>("""
        SELECT COUNT(*) AS Value FROM sys.tables WHERE name IN (N'JobSchedules', N'JobExecutions')
        """).SingleAsync();

    private sealed class CancelSecondTable(CancellationTokenSource cancellation) : DbCommandInterceptor
    {
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.StartsWith("CREATE TABLE [JobSchedules]", StringComparison.Ordinal))
            {
                cancellation.Cancel();
                cancellationToken.ThrowIfCancellationRequested();
            }
            return ValueTask.FromResult(result);
        }
    }
}
