using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
    public async Task Domain_service_finishes_each_removal_and_save_before_completing()
    {
        var repository = Substitute.For<IDomainRepository>();
        var inactive = new Project.Domain.Entities.Domain { Id = 1, IsActive = false };
        var active = new Project.Domain.Entities.Domain { Id = 2, IsActive = true };
        repository.GetAllQueryable().Returns(new[] { inactive, active }.AsQueryable());
        var removal = NewCompletion();
        var saveStarted = NewCompletion();
        var save = NewCompletion();
        repository.RemoveWithoutSaveChange(inactive).Returns(removal.Task);
        repository.SaveChangesTask().Returns(_ => { saveStarted.SetResult(); return save.Task; });
        var cloudflare = new CloudflareApiClient(Options.Create(new CloudflareSettings
            { Email = "synthetic@example.invalid", ApiKey = "synthetic-test-key" }));
        var service = new DomainService(Substitute.For<IMapper>(), repository, Substitute.For<IServerService>(),
            new ConfigurationBuilder().Build(), cloudflare);
        var work = service.Remove();
        try
        {
            Assert.False(work.IsCompleted);
            Assert.False(saveStarted.Task.IsCompleted);
            removal.SetResult();
            await saveStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(work.IsCompleted);
            save.SetResult();
            await work;
            await repository.DidNotReceive().RemoveWithoutSaveChange(active);
            await repository.Received(1).SaveChangesTask();
        }
        finally
        {
            removal.TrySetResult();
            save.TrySetResult();
        }
    }

    private static void AssertSuccess(IActionResult result) => Assert.True(JToken.DeepEquals(
        JObject.Parse("{\"status\":\"1\",\"message\":\"done successfully\"}"),
        JObject.FromObject(Assert.IsType<JsonResult>(result).Value!)));
}
