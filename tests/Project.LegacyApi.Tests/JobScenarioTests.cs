using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Project.Application.DTOs.Server;
using Project.Application.DTOs.ServerLog;
using Project.Application.Features.Interfaces;
using Project.Application.Jobs;
using Xunit;

namespace Project.LegacyApi.Tests;

public sealed class JobScenarioTests
{
    [Theory]
    [InlineData(20, 10, 10, true)]
    [InlineData(5, 10, 10, false)]
    [InlineData(20, 9, 11, false)]
    [InlineData(20, 5, 5, false)]
    public async Task Domain_rotation_keeps_the_age_count_and_rounded_percentage_rules(int age, int failures, int successes, bool rotates)
    {
        var servers = Substitute.For<IServerService>();
        var domains = Substitute.For<IDomainService>();
        servers.GetActiveIds().Returns(Task.FromResult(new List<int> { 7 }));
        servers.GetServerStatistics(7).Returns(Task.FromResult(new ServerDTO
        {
            Id = 7, DomainDateTime = DateTime.UtcNow.AddMinutes(-age),
            AllLogsStatistics = new ServerLogStatistics { Count = failures + successes, FailCount = failures, SuccessCount = successes }
        }));
        using var services = new ServiceCollection().AddSingleton(servers).AddSingleton(domains).BuildServiceProvider();
        var handler = new JobHandler(services, TimeProvider.System);
        await handler.ExecuteAsync(new JobClaim(Guid.NewGuid(), JobCatalog.Domain, 1,
            new JobSettings(true, 10, 10, 10, 50)), CancellationToken.None);
        await domains.Received(rotates ? 1 : 0).ChangeDomain("7");
    }

    [Theory]
    [InlineData(true, 20, true)]
    [InlineData(true, 5, false)]
    [InlineData(false, 20, false)]
    public async Task Ip_rotation_requires_statistics_and_an_expired_domain(bool hasStatistics, int age, bool rotates)
    {
        var servers = Substitute.For<IServerService>();
        var configs = Substitute.For<IIPConfigService>();
        servers.GetActiveIds().Returns(Task.FromResult(new List<int> { 7 }));
        servers.GetServerStatistics(7).Returns(Task.FromResult(new ServerDTO
        {
            Id = 7, DomainDateTime = DateTime.UtcNow.AddMinutes(-age),
            AllLogsStatistics = hasStatistics ? new ServerLogStatistics { Count = 1 } : null!
        }));
        using var services = new ServiceCollection().AddSingleton(servers).AddSingleton(configs).BuildServiceProvider();
        var handler = new JobHandler(services, TimeProvider.System);
        await handler.ExecuteAsync(new JobClaim(Guid.NewGuid(), JobCatalog.IpConfig, 1,
            new JobSettings(true, 10, 10, 10, 50)), CancellationToken.None);
        await configs.Received(rotates ? 1 : 0).GenerateUpdateAsync("7", null!, null!);
    }
}
