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

        public async Task Reset()
        {
            var serverLogService = (IServerLogService)serviceProvider.GetService(typeof(IServerLogService))!;
            await serverLogService.DeleteServerLogs();
        }
    }
}
