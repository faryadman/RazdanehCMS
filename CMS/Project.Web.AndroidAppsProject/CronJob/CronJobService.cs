using Project.Application.Features.Interfaces;

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
            var serverChangeSubDomainService = (IDomainService)serviceProvider.GetService(typeof(IDomainService))!;
            await serverChangeSubDomainService.ChangeSubDomain();
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
