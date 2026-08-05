namespace Project.Web.AndroidAppsProject.CronJob
{
    public interface ICronJobService
    {
        Task ResetServerLog();
        Task ResetServerSubDomain();
        Task ResetServerDomain();
        Task ResetServerDns();
        Task CheckZoneId();
    }
}