using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Project.Application.Contracts.Persistence;
using Project.Application.Features;
using Project.Application.Features.Interfaces;
using Project.Application.Features.Services;
using Project.Domain.Entities;
using Project.Web.AndroidAppsProject.Areas.Admin.Controllers;
using Xunit;

namespace Project.LegacyApi.Tests;

public sealed class AdminDeletionLifetimeTests
{
    [Fact]
    public async Task Inactive_ip_deletions_finish_in_sequence_before_returning_success()
    {
        var service = Substitute.For<IIPConfigService>();
        service.ListInactive().Returns(new[] { new IPConfigEntity { Id = 1 }, new IPConfigEntity { Id = 2 } });
        var first = NewCompletion();
        var second = NewCompletion();
        var secondStarted = NewCompletion();
        service.Delete(1).Returns(first.Task);
        service.Delete(2).Returns(_ => { secondStarted.SetResult(); return second.Task; });
        var controller = new IPConfigController(Substitute.For<IWebHostEnvironment>(), service);
        var action = controller.DeleteInactive();
        try
        {
            Assert.False(action.IsCompleted);
            Assert.False(secondStarted.Task.IsCompleted);
            first.SetResult();
            await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(action.IsCompleted);
            second.SetResult();
            AssertSuccess(await action);
        }
        finally
        {
            first.TrySetResult();
            second.TrySetResult();
        }
    }

    [Fact]
    public async Task Failed_ip_deletion_is_observed_and_stops_the_next_deletion()
    {
        var service = Substitute.For<IIPConfigService>();
        service.ListInactive().Returns(new[] { new IPConfigEntity { Id = 1 }, new IPConfigEntity { Id = 2 } });
        var error = new InvalidOperationException("خطای ساختگی حذف");
        service.Delete(1).Returns(Task.FromException(error));
        var controller = new IPConfigController(Substitute.For<IWebHostEnvironment>(), service);
        Assert.Same(error, await Assert.ThrowsAsync<InvalidOperationException>(() => controller.DeleteInactive()));
        Assert.DoesNotContain(service.ReceivedCalls(), call => call.GetMethodInfo().Name == "Delete" && (int)call.GetArguments()[0]! == 2);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Domain_cleanup_is_awaited_and_preserves_success_or_failure(bool fails)
    {
        var service = Substitute.For<IDomainService>();
        var work = NewCompletion();
        service.Remove().Returns(work.Task);
        var controller = new DomainsController(service, Substitute.For<IWebHostEnvironment>());
        var action = controller.DeleteInactiveDomain();
        try
        {
            Assert.False(action.IsCompleted);
            if (fails)
            {
                var error = new InvalidOperationException("خطای ساختگی حذف");
                work.SetException(error);
                Assert.Same(error, await Assert.ThrowsAsync<InvalidOperationException>(() => action));
            }
            else
            {
                work.SetResult();
                AssertSuccess(await action);
            }
        }
        finally { work.TrySetResult(); }
    }

    private static TaskCompletionSource NewCompletion() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    [Fact]
    public async Task Domain_service_waits_for_the_database_cleanup_to_finish()
    {
        var repository = Substitute.For<IDomainRepository>();
        var delete = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        repository.DeleteInactiveAsync(Arg.Any<CancellationToken>()).Returns(delete.Task);
        var service = new DomainService(Substitute.For<IMapper>(), repository, Substitute.For<IServerService>(),
            Substitute.For<ICloudflareApiClient>());
        var work = service.Remove();
        try
        {
            Assert.False(work.IsCompleted);
            delete.SetResult(2);
            await work;
            await repository.Received(1).DeleteInactiveAsync(Arg.Any<CancellationToken>());
        }
        finally { delete.TrySetResult(0); }
    }

    private static void AssertSuccess(IActionResult result) => Assert.True(JToken.DeepEquals(
        JObject.Parse("{\"status\":\"1\",\"message\":\"done successfully\"}"),
        JObject.FromObject(Assert.IsType<JsonResult>(result).Value!)));
}
