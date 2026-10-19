using Project.Application.Features.Interfaces;
using Serilog;

namespace Project.Web.AndroidAppsProject.CronJob
{
    public class CronJobService : ICronJobService
    {
        private readonly IServiceScopeFactory scopeFactory;

        public CronJobService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public async Task ResetServerLog()
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var serverLogService = scope.ServiceProvider.GetRequiredService<IServerLogService>();
            await serverLogService.DeleteServerLogs();
        }

        public async Task ResetServerSubDomain()
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var serverChangeSubDomainService = scope.ServiceProvider.GetRequiredService<IDomainService>();
                await serverChangeSubDomainService.ChangeSubDomain();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        public async Task ResetServerDomain()
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var serverChangeDomainService = scope.ServiceProvider.GetRequiredService<IDomainService>();
                await serverChangeDomainService.ChangeDomain();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        public async Task ResetServerDns()
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var serverChangeSubDomainService = scope.ServiceProvider.GetRequiredService<IDomainService>();
            await serverChangeSubDomainService.DeleteDnsAsync();
        }

        public async Task CheckZoneId()
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var serverChangeSubDomainService = scope.ServiceProvider.GetRequiredService<IDomainService>();
            await serverChangeSubDomainService.InitZoneId();
        }
    }
}
