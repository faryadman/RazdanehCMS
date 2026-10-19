using Project.Application.Features.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Project.Application.Jobs;

public sealed class JobHandler(IServiceProvider services, TimeProvider clock) : IJobHandler
{
    public async Task ExecuteAsync(JobClaim claim, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        JobCatalog.Get(claim.JobName);
        switch (claim.JobName)
        {
            case JobCatalog.DeleteLogs:
                await services.GetRequiredService<IServerLogService>().DeleteServerLogsAsync(cancellationToken);
                return;
            case JobCatalog.CheckZones:
                await services.GetRequiredService<IDomainService>().InitZoneId();
                return;
            case JobCatalog.DeleteDns:
                await services.GetRequiredService<IDomainService>().DeleteDnsAsync(expireMinuteOn: claim.Settings.ExpireMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return;
        }
        var servers = services.GetRequiredService<IServerService>();
        foreach (var id in await servers.GetActiveIds())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (claim.JobName == JobCatalog.SubDomain)
            {
                await services.GetRequiredService<IDomainService>().ChangeSubDomain(id.ToString());
                continue;
            }
            var server = await servers.GetServerStatistics(id);
            var statistics = server?.AllLogsStatistics;
            if (server == null || statistics == null) continue;
            var age = (clock.GetUtcNow().UtcDateTime - server.DomainDateTime).TotalMinutes;
            if (age <= claim.Settings.ExpireMinutes) continue;
            cancellationToken.ThrowIfCancellationRequested();
            if (claim.JobName == JobCatalog.IpConfig)
                await services.GetRequiredService<IIPConfigService>().GenerateUpdateAsync(id.ToString(), null, null);
            else if (claim.JobName == JobCatalog.Domain && statistics.Count > 0
                && statistics.FailCount >= claim.Settings.FailureCount
                && Math.Round(100d * statistics.FailCount / statistics.Count) >= claim.Settings.FailurePercent)
                await services.GetRequiredService<IDomainService>().ChangeDomain(id.ToString());
        }
    }
}
