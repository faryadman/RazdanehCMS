using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Features.Interfaces;
using Project.Persistence;

namespace Project.Web.AndroidAppsProject.Controllers
{
    public class LogDeleterController : Controller
    {
        private readonly IApiLogService _apiLogService;
        private readonly IServerLogService _serverLogService;
        private readonly ApplicationDbContext _context;

        public LogDeleterController(ApplicationDbContext context, IApiLogService apiLogService, IServerLogService serverLogService)
        {
            _context = context;
            _apiLogService = apiLogService;
            _serverLogService = serverLogService;
            RecurringJob.AddOrUpdate("deleteApiLogJob", () => Index(), "*/10 * * * *");
            RecurringJob.AddOrUpdate("deleteServerLogsJob", () => Compress(), "*/10 * * * *");
        }
        public async Task<IActionResult> Index()
        {
            await _apiLogService.DeleteApiLog();
            return Ok(true);
        }
        public async Task<IActionResult> Compress()
        {
            var list = _context.ServerLogs.ToList();
            foreach (var log in list)
            {
                _context.ServerLogs.Remove(log);
            }
            await _context.SaveChangesAsync();
            return Ok(true);
        }
    }
}