using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Features.Interfaces;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly IServerService _serverService;
        private readonly IServerLogService _serverLogService;

        public DashboardController(IServerService serverService, IServerLogService serverLogService)
        {
            _serverService = serverService;
            _serverLogService = serverLogService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // داده‌های داشبورد — همه از همان سرویس‌هایی که صفحات دیگر استفاده می‌کنند
        public async Task<IActionResult> GetStats()
        {
            var servers = await _serverService.GetWithFilter(null, null, false, 0);
            var stats = await _serverLogService.GetAllLogsStatistics();

            var model = new
            {
                totalServers = servers.Count,
                availableServers = servers.Count(s => s.IsAvailable),
                adServers = servers.Count(s => s.IsAd),
                all = new
                {
                    count = stats?.AllLogsStatistics?.Count ?? 0,
                    success = stats?.AllLogsStatistics?.SuccessCount ?? 0,
                    fail = stats?.AllLogsStatistics?.FailCount ?? 0
                },
                irancell = new
                {
                    count = stats?.IrancellLogsStatistics?.Count ?? 0,
                    success = stats?.IrancellLogsStatistics?.SuccessCount ?? 0,
                    fail = stats?.IrancellLogsStatistics?.FailCount ?? 0
                },
                hamraheAvval = new
                {
                    count = stats?.HamraheAvvalLogsStatistics?.Count ?? 0,
                    success = stats?.HamraheAvvalLogsStatistics?.SuccessCount ?? 0,
                    fail = stats?.HamraheAvvalLogsStatistics?.FailCount ?? 0
                },
                unknown = new
                {
                    count = stats?.UnknownLogsStatistics?.Count ?? 0,
                    success = stats?.UnknownLogsStatistics?.SuccessCount ?? 0,
                    fail = stats?.UnknownLogsStatistics?.FailCount ?? 0
                },
                servers = servers
                    .Select(s => new
                    {
                        id = s.Id,
                        name = s.ServerName,
                        isAd = s.IsAd,
                        isAvailable = s.IsAvailable,
                        count = s.AllLogsStatistics?.Count ?? 0,
                        successCount = s.AllLogsStatistics?.SuccessCount ?? 0
                    })
                    .OrderByDescending(x => x.count)
                    .Take(10)
                    .ToList()
            };

            return Json(model);
        }
    }
}
