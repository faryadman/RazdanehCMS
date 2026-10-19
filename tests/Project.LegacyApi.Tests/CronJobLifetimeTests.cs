using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Project.Application.Features.Interfaces;
using Project.Web.AndroidAppsProject.CronJob;
using Xunit;

namespace Project.LegacyApi.Tests;

public sealed class CronJobLifetimeTests
{
    [Theory]
    [InlineData("logs", false)]
    [InlineData("subdomain", false)]
    [InlineData("domain", false)]
    [InlineData("dns", false)]
    [InlineData("zone", false)]
    [InlineData("logs", true)]
    [InlineData("subdomain", true)]
    [InlineData("domain", true)]
    [InlineData("dns", true)]
    [InlineData("zone", true)]
    public async Task Each_job_awaits_work_and_async_disposal_even_when_work_fails(string name, bool fails)
    {
        var resources = new ConcurrentQueue<JobResource>();
        await using var provider = CreateProvider(resources);
        var job = provider.GetRequiredService<ICronJobService>();
        var run = Invoke(job, name);
        try
        {
            var resource = Assert.Single(resources);
            Assert.False(run.IsCompleted);
            Assert.False(resource.DisposalStarted.Task.IsCompleted);
            var error = new InvalidOperationException("خطای ساختگی عملیات");
            if (fails) resource.Work.SetException(error);
            else resource.Work.SetResult();

            await resource.DisposalStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(run.IsCompleted);
            resource.FinishDisposal.SetResult();
            if (fails && name is not ("domain" or "subdomain"))
                Assert.Same(error, await Assert.ThrowsAsync<InvalidOperationException>(() => run));
            else
                await run;
            Assert.Equal(1, resource.DisposalCount);
        }
        finally
        {
            foreach (var resource in resources)
            {
                resource.Work.TrySetResult();
                resource.FinishDisposal.TrySetResult();
            }
        }
    }

    [Fact]
    public async Task Concurrent_invocations_have_independent_scoped_resources()
    {
        var resources = new ConcurrentQueue<JobResource>();
        await using var provider = CreateProvider(resources);
        var job = provider.GetRequiredService<ICronJobService>();
        var first = job.ResetServerLog();
        var second = job.ResetServerLog();
        try
        {
            var scopes = resources.ToArray();
            Assert.Equal(2, scopes.Length);
            Assert.NotSame(scopes[0], scopes[1]);
            scopes[0].Work.SetResult();
            scopes[0].FinishDisposal.SetResult();
            await first;
            Assert.Equal(1, scopes[0].DisposalCount);
            Assert.False(second.IsCompleted);
            Assert.False(scopes[1].DisposalStarted.Task.IsCompleted);
            scopes[1].Work.SetResult();
            scopes[1].FinishDisposal.SetResult();
            await second;
            Assert.Equal(1, scopes[1].DisposalCount);
        }
        finally
        {
            foreach (var resource in resources)
            {
                resource.Work.TrySetResult();
                resource.FinishDisposal.TrySetResult();
            }
        }
    }

    private static Task Invoke(ICronJobService job, string name) => name switch
    {
        "logs" => job.ResetServerLog(),
        "subdomain" => job.ResetServerSubDomain(),
        "domain" => job.ResetServerDomain(),
        "dns" => job.ResetServerDns(),
        "zone" => job.CheckZoneId(),
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };

    private static ServiceProvider CreateProvider(ConcurrentQueue<JobResource> resources)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ =>
        {
            var resource = new JobResource();
            resources.Enqueue(resource);
            return resource;
        });
        services.AddScoped(provider =>
        {
            var resource = provider.GetRequiredService<JobResource>();
            var service = Substitute.For<IServerLogService>();
            service.DeleteServerLogs().Returns(_ => resource.Work.Task);
            return service;
        });
        services.AddScoped(provider =>
        {
            var resource = provider.GetRequiredService<JobResource>();
            var service = Substitute.For<IDomainService>();
            service.ChangeSubDomain().Returns(_ => resource.Work.Task);
            service.ChangeDomain().Returns(_ => resource.Work.Task);
            service.DeleteDnsAsync().Returns(async _ =>
            {
                await resource.Work.Task;
                return "";
            });
            service.InitZoneId().Returns(_ => resource.Work.Task);
            return service;
        });
        services.AddSingleton<ICronJobService, CronJobService>();
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    private sealed class JobResource : IAsyncDisposable
    {
        public TaskCompletionSource Work { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource DisposalStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource FinishDisposal { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int DisposalCount { get; private set; }

        public async ValueTask DisposeAsync()
        {
            DisposalStarted.SetResult();
            await FinishDisposal.Task;
            DisposalCount++;
        }
    }
}
