namespace Project.Web.AndroidAppsProject.CronJob
{
    public interface ICronJobService
    {
        Task ResetServerLog();
        Task ResetServerSubDomain();
        Task ResetServerDns();
        Task CheckZoneId();
    }
}
