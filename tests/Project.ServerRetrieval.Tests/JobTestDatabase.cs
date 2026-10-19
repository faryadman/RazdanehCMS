using System.Collections.Concurrent;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Project.Application.DTOs.Job.DomainJob;
using Project.Application.Jobs;
using Project.Persistence;
using Project.Web.AndroidAppsProject.Jobs;

namespace Project.ServerRetrieval.Tests;

public sealed class JobTestDatabase : IAsyncDisposable
{
    public string ConnectionString { get; }
    public ApplicationDbContext Context { get; }
    public ServiceProvider Services { get; }
    public ExecutionTracker Tracker { get; } = new();
    public NativeJobWorker Worker => Services.GetServices<IHostedService>().OfType<NativeJobWorker>().Single();

    private JobTestDatabase(string containerConnectionString)
    {
        ConnectionString = new SqlConnectionStringBuilder(containerConnectionString)
            { InitialCatalog = "jobs_" + Guid.NewGuid().ToString("N") }.ConnectionString;
        Context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(ConnectionString).Options);
        Services = CreateProvider();
    }

    public static async Task<JobTestDatabase> CreateAsync(string connectionString, bool initialize = true)
    {
        var database = new JobTestDatabase(connectionString);
        try
        {
            await database.Context.Database.EnsureCreatedAsync();
            if (initialize)
            {
                await database.ReadAsync();
                await database.SaveAsync(JobCatalog.DeleteLogs, false);
                await database.SaveAsync(JobCatalog.CheckZones, false);
            }
            return database;
        }
        catch { await database.DisposeAsync(); throw; }
    }

    public ServiceProvider CreateProvider()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = ConnectionString,
            ["NativeJobs:PollSeconds"] = "1"
        }).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.ConfigurePersistenceServices(configuration);
        services.AddScoped<JobExecutionScope>();
        services.AddScoped<IJobHandler>(_ => new RecordingHandler(Tracker));
        services.AddNativeJobs(configuration);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    public async Task SaveAsync(string name, bool enabled, int period = 10)
    {
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IJobStore>().SaveAsync(name,
            new CreateDomainJobDTO { IsActiveJob = enabled, JobPeriodTime = period, JobExpireMinuteTime = 30, FailConnectionCount = 10, FailConnectionPercent = 50 },
            "test-admin", CancellationToken.None);
    }

    public async Task RequestAsync(string name)
    {
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IJobStore>().RequestRunAsync(name, CancellationToken.None);
    }

    public async Task<JobDashboard> ReadAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IJobStore>().GetDashboardAsync(CancellationToken.None);
    }

    public async ValueTask DisposeAsync()
    {
        try { await Services.DisposeAsync(); }
        finally { await Context.DisposeAsync(); }
    }

    private sealed class RecordingHandler(ExecutionTracker tracker) : IJobHandler
    {
        public async Task ExecuteAsync(JobClaim claim, CancellationToken cancellationToken)
        {
            tracker.Claims.Enqueue(claim);
            var active = Interlocked.Increment(ref tracker.Active);
            int maximum;
            do { maximum = tracker.MaximumActive; }
            while (active > maximum && Interlocked.CompareExchange(ref tracker.MaximumActive, active, maximum) != maximum);
            try { await tracker.Execute(claim, cancellationToken); }
            finally { Interlocked.Decrement(ref tracker.Active); }
        }
    }
}

public sealed class ExecutionTracker
{
    public ConcurrentQueue<JobClaim> Claims { get; } = new();
    public Func<JobClaim, CancellationToken, Task> Execute { get; set; } = (_, _) => Task.CompletedTask;
    public int Active;
    public int MaximumActive;
}
