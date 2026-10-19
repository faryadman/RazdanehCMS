using Microsoft.Extensions.Options;
using Project.Application.Jobs;
using Project.Domain.Jobs;

namespace Project.Web.AndroidAppsProject.Jobs;

public sealed class NativeJobOptions
{
    public bool Enabled { get; set; } = true;
    public double PollSeconds { get; set; } = 5;
    public double ExecutionTimeoutMinutes { get; set; } = 15;
}

public sealed class NativeJobWorker(IServiceScopeFactory scopes, IJobExecutionLock executionLock,
    IOptions<NativeJobOptions> options, ILogger<NativeJobWorker> logger) : BackgroundService
{
    private readonly string workerId = Guid.NewGuid().ToString("N");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled) return;
        var databaseUnavailable = false;
        while (!stoppingToken.IsCancellationRequested)
        {
            var delaySeconds = options.Value.PollSeconds;
            try
            {
                var executed = await ExecuteNextAsync(stoppingToken);
                databaseUnavailable = false;
                if (executed) continue;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (JobSchemaNotReadyException exception)
            {
                if (!databaseUnavailable) logger.LogError("{SetupMessage}", exception.Message);
                databaseUnavailable = true;
                delaySeconds = Math.Max(30, delaySeconds);
            }
            catch (Exception exception)
            {
                // Never put configuration, credentials, provider URLs or raw exception messages in job logs/history.
                if (!databaseUnavailable)
                    logger.LogError("زمان‌بند جاب به دیتابیس دسترسی ندارد یا schema آن آماده نیست. نوع خطا: {ErrorType}", exception.GetType().Name);
                databaseUnavailable = true;
                delaySeconds = Math.Max(30, delaySeconds);
            }
            try { await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }

    public async Task<bool> ExecuteNextAsync(CancellationToken stoppingToken)
    {
        await using var lease = await executionLock.TryAcquireAsync(stoppingToken);
        if (lease == null) return false;
        JobClaim? claim;
        await using (var scope = scopes.CreateAsyncScope())
            claim = await scope.ServiceProvider.GetRequiredService<IJobStore>().ClaimNextAsync(workerId, stoppingToken);
        if (claim == null) return false;
        lease.WatchConfiguration(claim);
        using var executionCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, lease.CancellationToken);
        executionCancellation.CancelAfter(TimeSpan.FromMinutes(options.Value.ExecutionTimeoutMinutes));
        var status = JobExecutionStatus.Succeeded;
        string? errorCode = null;
        try
        {
            // Resolve after setting the token, so scoped business services see the execution's lifetime.
            await using var scope = scopes.CreateAsyncScope();
            scope.ServiceProvider.GetRequiredService<JobExecutionScope>().CancellationToken = executionCancellation.Token;
            executionCancellation.Token.ThrowIfCancellationRequested();
            await scope.ServiceProvider.GetRequiredService<IJobHandler>().ExecuteAsync(claim, executionCancellation.Token);
            executionCancellation.Token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException) when (executionCancellation.IsCancellationRequested)
        {
            status = JobExecutionStatus.Cancelled;
            errorCode = stoppingToken.IsCancellationRequested ? "shutdown"
                : lease.ConfigurationChanged ? "configuration_changed" : "execution_timeout";
        }
        catch (Exception)
        {
            status = JobExecutionStatus.Failed;
            errorCode = "execution_failed";
        }
        if (lease.ConnectionLost)
        {
            status = JobExecutionStatus.Interrupted;
            errorCode = "connection_lost";
        }
        // Do not use WaitAsync to abandon a handler on timeout. Keep the lock until the actual task and scope finish.
        using var completionTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await using (var scope = scopes.CreateAsyncScope())
        {
            var requiresReview = status == JobExecutionStatus.Interrupted
                || (status != JobExecutionStatus.Succeeded && JobCatalog.Get(claim.JobName).RequiresReviewAfterFailure);
            await scope.ServiceProvider.GetRequiredService<IJobStore>().CompleteAsync(claim, status, errorCode, requiresReview, completionTimeout.Token);
        }
        logger.LogInformation("اجرای جاب {JobName} با شناسهٔ {ExecutionId} و وضعیت {Status} پایان یافت.", claim.JobName, claim.ExecutionId, status);
        return true;
    }
}
