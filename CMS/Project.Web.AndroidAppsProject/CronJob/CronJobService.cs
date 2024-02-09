using Project.Application.Features.Interfaces;
using Serilog;

namespace Project.Web.AndroidAppsProject.CronJob
{
    public class CronJobService : ICronJobService
    {
        private readonly IServiceProvider serviceProvider;

        public CronJobService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task ResetServerLog()
        {
            var serverLogService = (IServerLogService)serviceProvider.GetService(typeof(IServerLogService))!;
            await serverLogService.DeleteServerLogs();
        }

        public async Task ResetServerSubDomain()
        {
            try
            {
                var serverChangeSubDomainService = (IDomainService)serviceProvider.GetService(typeof(IDomainService))!;
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
                var serverChangeDomainService = (IDomainService)serviceProvider.GetService(typeof(IDomainService))!;
                await serverChangeDomainService.ChangeDomain();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        public async Task ResetServerDns()
        {
            var serverChangeSubDomainService = (IDomainService)serviceProvider.GetService(typeof(IDomainService))!;
            await serverChangeSubDomainService.DeleteDnsAsync();
        }

        public async Task CheckZoneId()
        {
            var serverChangeSubDomainService = (IDomainService)serviceProvider.GetService(typeof(IDomainService))!;
            await serverChangeSubDomainService.InitZoneId();
        }
    }
}