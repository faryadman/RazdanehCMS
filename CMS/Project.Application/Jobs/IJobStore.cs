#nullable enable
using Project.Application.DTOs.Job.DomainJob;
using Project.Domain.Jobs;

namespace Project.Application.Jobs;

public sealed record JobClaim(Guid ExecutionId, string JobName, int ConfigurationId, JobSettings Settings, string ConfigurationHash = "");
public sealed record JobOverview(JobDefinition Definition, JobSettings Settings, JobSchedule Schedule);
public sealed record JobDashboard(IReadOnlyList<JobOverview> Jobs, IReadOnlyList<JobExecution> History, bool NeedsReview);

public interface IJobStore
{
    Task<JobDashboard> GetDashboardAsync(CancellationToken cancellationToken);
    Task SaveAsync(string name, CreateDomainJobDTO input, string actor, CancellationToken cancellationToken);
    Task RequestRunAsync(string name, CancellationToken cancellationToken);
    // Called only while owning the database-wide execution lock.
    Task<JobClaim?> ClaimNextAsync(string workerId, CancellationToken cancellationToken);
    Task CompleteAsync(JobClaim claim, string status, string? errorCode, bool requiresReview, CancellationToken cancellationToken);
    Task ReviewAsync(Guid executionId, string actor, CancellationToken cancellationToken);
}

public interface IJobExecutionLock
{
    Task<IJobLease?> TryAcquireAsync(CancellationToken cancellationToken);
}

public interface IJobLease : IAsyncDisposable
{
    CancellationToken CancellationToken { get; }
    bool ConnectionLost { get; }
    bool ConfigurationChanged { get; }
    void WatchConfiguration(JobClaim claim);
}

public interface IJobHandler
{
    Task ExecuteAsync(JobClaim claim, CancellationToken cancellationToken);
}

// A fresh DI instance is set before resolving the business services for each execution.
public sealed class JobExecutionScope
{
    public CancellationToken CancellationToken { get; set; }
}
