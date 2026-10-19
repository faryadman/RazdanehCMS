using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Project.Application.Jobs;
using Project.Domain.Jobs;
using Project.Web.AndroidAppsProject.Jobs;
using Xunit;

namespace Project.LegacyApi.Tests;

public sealed class NativeJobLifetimeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Scope_disposal_and_completion_finish_before_the_execution_lock_is_released(bool fails)
    {
        var state = new State { Fails = fails };
        await using var provider = CreateProvider(state);
        using var worker = CreateWorker(provider, state);
        var run = worker.ExecuteNextAsync(CancellationToken.None);
        try
        {
            await state.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(state.Lock.Held);
            Assert.False(state.DisposalStarted.Task.IsCompleted);
            state.FinishWork.SetResult();
            await state.DisposalStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(run.IsCompleted);
            Assert.True(state.Lock.Held);
            Assert.DoesNotContain(state.Store.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IJobStore.CompleteAsync));
            state.FinishDisposal.SetResult();
            Assert.True(await run);
            Assert.Equal(1, state.Disposals);
            Assert.False(state.Lock.Held);
            await state.Store.Received(1).CompleteAsync(Arg.Any<JobClaim>(),
                fails ? JobExecutionStatus.Failed : JobExecutionStatus.Succeeded,
                fails ? "execution_failed" : null, fails, Arg.Any<CancellationToken>());
        }
        finally { state.Release(); }
    }

    [Fact]
    public async Task Hosted_worker_shutdown_cancels_but_does_not_abandon_an_uncooperative_handler()
    {
        var state = new State();
        await using var provider = CreateProvider(state);
        using var worker = CreateWorker(provider, state);
        Task? stop = null;
        try
        {
            await worker.StartAsync(CancellationToken.None);
            await state.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            stop = worker.StopAsync(CancellationToken.None);
            Assert.True(state.ExecutionToken.IsCancellationRequested);
            Assert.False(stop.IsCompleted);
            Assert.True(state.Lock.Held);
            state.FinishWork.SetResult();
            await state.DisposalStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(stop.IsCompleted);
            Assert.True(state.Lock.Held);
            state.FinishDisposal.SetResult();
            await stop.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(state.Lock.Held);
            await state.Store.Received(1).CompleteAsync(Arg.Any<JobClaim>(), JobExecutionStatus.Cancelled,
                "shutdown", true, Arg.Any<CancellationToken>());
        }
        finally
        {
            state.Release();
            if (stop != null) await stop;
        }
    }

    private static ServiceProvider CreateProvider(State state)
    {
        var services = new ServiceCollection();
        services.AddSingleton(state.Store);
        services.AddScoped<JobExecutionScope>();
        services.AddScoped<IJobHandler>(provider => new Handler(state, provider.GetRequiredService<JobExecutionScope>()));
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    [Fact]
    public async Task Timeout_cancels_cooperatively_and_keeps_ownership_until_the_handler_returns()
    {
        var state = new State();
        await using var provider = CreateProvider(state);
        using var worker = CreateWorker(provider, state, timeoutMinutes: 0.02);
        var run = worker.ExecuteNextAsync(CancellationToken.None);
        try
        {
            await state.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var cancelled = NewCompletion();
            using var registration = state.ExecutionToken.Register(() => cancelled.TrySetResult());
            await cancelled.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(state.Lock.Held);
            Assert.False(run.IsCompleted);
            state.Release();
            Assert.True(await run);
            await state.Store.Received(1).CompleteAsync(Arg.Any<JobClaim>(), JobExecutionStatus.Cancelled,
                "execution_timeout", true, Arg.Any<CancellationToken>());
        }
        finally { state.Release(); }
    }

    private static NativeJobWorker CreateWorker(ServiceProvider provider, State state, double timeoutMinutes = 15) => new(
        provider.GetRequiredService<IServiceScopeFactory>(), state.Lock,
        Options.Create(new NativeJobOptions { PollSeconds = 0.02, ExecutionTimeoutMinutes = timeoutMinutes }), NullLogger<NativeJobWorker>.Instance);

    private sealed class State
    {
        public bool Fails;
        public CancellationToken ExecutionToken;
        public int Disposals;
        public TaskCompletionSource Entered { get; } = NewCompletion();
        public TaskCompletionSource FinishWork { get; } = NewCompletion();
        public TaskCompletionSource DisposalStarted { get; } = NewCompletion();
        public TaskCompletionSource FinishDisposal { get; } = NewCompletion();
        public TestLock Lock { get; } = new();
        public IJobStore Store { get; } = Substitute.For<IJobStore>();
        public State()
        {
            var claim = new JobClaim(Guid.NewGuid(), JobCatalog.Domain, 1, JobSettings.Default(JobCatalog.Domain) with { Enabled = true });
            var claims = 0;
            Store.ClaimNextAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(_ => Task.FromResult(Interlocked.Increment(ref claims) == 1 ? claim : null));
        }
        public void Release() { FinishWork.TrySetResult(); FinishDisposal.TrySetResult(); }
    }

    private sealed class Handler(State state, JobExecutionScope scope) : IJobHandler, IAsyncDisposable
    {
        public async Task ExecuteAsync(JobClaim claim, CancellationToken cancellationToken)
        {
            Assert.Equal(cancellationToken, scope.CancellationToken);
            state.ExecutionToken = cancellationToken;
            state.Entered.SetResult();
            await state.FinishWork.Task; // Deliberately ignores cancellation to verify ownership until actual completion.
            if (state.Fails) throw new InvalidOperationException("خطای ساختگی محرمانه که نباید در history ذخیره شود");
        }
        public async ValueTask DisposeAsync()
        {
            state.DisposalStarted.SetResult();
            await state.FinishDisposal.Task;
            state.Disposals++;
        }
    }

    private sealed class TestLock : IJobExecutionLock
    {
        public bool Held;
        public Task<IJobLease?> TryAcquireAsync(CancellationToken cancellationToken)
        {
            Assert.False(Held);
            Held = true;
            return Task.FromResult<IJobLease?>(new Lease(this));
        }
        private sealed class Lease(TestLock owner) : IJobLease
        {
            public CancellationToken CancellationToken => System.Threading.CancellationToken.None;
            public bool ConnectionLost => false;
            public bool ConfigurationChanged => false;
            public void WatchConfiguration(JobClaim claim) { }
            public ValueTask DisposeAsync() { owner.Held = false; return ValueTask.CompletedTask; }
        }
    }
    private static TaskCompletionSource NewCompletion() => new(TaskCreationOptions.RunContinuationsAsynchronously);
}
