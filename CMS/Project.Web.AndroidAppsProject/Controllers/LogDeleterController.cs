using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Features.Interfaces;

namespace Project.Web.AndroidAppsProject.Controllers
{
    public class LogDeleterController : Controller
    {
        private readonly IServerLogService _serverLogService;
        public LogDeleterController(IServerLogService serverLogService)
        {
            _serverLogService = serverLogService;
            RecurringJob.AddOrUpdate(
                "logDeleterJob",
                () => Index(),
            Cron.MinuteInterval(10));
        }
        public async Task<IActionResult> Index()
        {
            await _serverLogService.DeleteServerLogs();
            return Ok(true);
        }
    }
}