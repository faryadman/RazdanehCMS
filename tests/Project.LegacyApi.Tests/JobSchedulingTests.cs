using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Project.Application.DTOs.Job.DomainJob;
using Project.Application.Exceptions;
using Project.Application.Features.Interfaces;
using Project.Application.Jobs;
using Project.Domain.Entities;
using Xunit;

namespace Project.LegacyApi.Tests;

public sealed class JobSchedulingTests
{
    [Theory]
    [InlineData("2026-10-19T12:00:00Z", 45, "2026-10-19T12:45:00Z")]
    [InlineData("2026-10-19T12:44:59Z", 45, "2026-10-19T12:45:00Z")]
    [InlineData("2026-10-19T12:45:00Z", 45, "2026-10-19T13:00:00Z")]
    [InlineData("2026-10-19T12:59:59Z", 45, "2026-10-19T13:00:00Z")]
    [InlineData("2026-10-19T12:15:01Z", 10, "2026-10-19T12:20:00Z")]
    [InlineData("2026-12-31T23:59:59Z", 10, "2027-01-01T00:00:00Z")]
    [InlineData("2028-02-28T23:59:59Z", 1, "2028-02-29T00:00:00Z")]
    public void Next_slot_preserves_utc_minute_cron_semantics(string now, int step, string next)
        => Assert.Equal(DateTime.Parse(next).ToUniversalTime(), MinuteSchedule.Next(DateTime.Parse(now).ToUniversalTime(), step));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(60)]
    [InlineData(100)]
    [InlineData(null)]
    public void Invalid_period_is_rejected_before_persisting(int? period) => Assert.Throws<BadRequestException>(() =>
        JobSettings.FromInput(JobCatalog.Domain, new CreateDomainJobDTO { IsActiveJob = true, JobPeriodTime = period,
            JobExpireMinuteTime = 30, FailConnectionCount = 10, FailConnectionPercent = 50 }));

    [Theory]
    [InlineData(JobCatalog.Domain)]
    [InlineData(JobCatalog.IpConfig)]
    [InlineData(JobCatalog.DeleteDns)]
    public void Active_legacy_jobs_with_missing_thresholds_do_not_silently_acquire_new_defaults(string name)
        => Assert.Throws<BadRequestException>(() => JobSettings.Read(new Job { JobName = name, IsActive = true,
            JobPeriodTime = 10, JobConfig = "{}" }));

    [Fact]
    public void Database_enabled_state_overrides_stale_enabled_flag_in_the_legacy_json()
    {
        var job = new Job { JobName = JobCatalog.Domain, IsActive = false, JobPeriodTime = 20,
            JobConfig = "{\"IsActiveJob\":true,\"JobPeriodTime\":5}" };
        var settings = JobSettings.Read(job);
        Assert.False(settings.Enabled);
        Assert.Equal(20, settings.MinuteStep);
    }

    [Fact]
    public async Task Pre_cancelled_execution_never_resolves_or_calls_a_business_service()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => new JobHandler(services, TimeProvider.System).ExecuteAsync(
            new JobClaim(Guid.NewGuid(), JobCatalog.SubDomain, 1, JobSettings.Default(JobCatalog.SubDomain)), cancellation.Token));
    }

    [Fact]
    public async Task Log_cleanup_does_not_require_cloudflare_or_server_services()
    {
        var logs = Substitute.For<IServerLogService>();
        using var services = new ServiceCollection().AddSingleton(logs).BuildServiceProvider();
        await new JobHandler(services, TimeProvider.System).ExecuteAsync(
            new JobClaim(Guid.NewGuid(), JobCatalog.DeleteLogs, 1, JobSettings.Default(JobCatalog.DeleteLogs)), CancellationToken.None);
        await logs.Received(1).DeleteServerLogsAsync(CancellationToken.None);
    }
}
