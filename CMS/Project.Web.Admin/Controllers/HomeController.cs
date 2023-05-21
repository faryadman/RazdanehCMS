using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Features.Interfaces;
using System.ComponentModel;

namespace Project.Web.Admin.Controllers
{
    [Authorize(Policy = Models.ConstantPolicies.DynamicPermission)]
    [DisplayName("داشبورد")]
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [DisplayName("صفحه اصلی")]
        public async Task<IActionResult> Index()
        {
            return Ok(1);
            var data = await _dashboardService.Statistics();
            return View(data);
        }

        [HttpGet("/transactionschartdata")]
        public async Task<JsonResult> TransactionsChartData()
        {
            var res = await _dashboardService.TransactionsChart();
            return Json(res);
        }

        [HttpGet("/transactionspiechartdata")]
        public async Task<JsonResult> TransactionsPieChartData()
        {
            var res = await _dashboardService.TransactionsPieChart();
            return Json(res);
        }

        [HttpGet("/transactionschartpercitydata")]
        public async Task<JsonResult> TransactionsChartPerCityData()
        {
            var res = await _dashboardService.TransactionsChartPerCity();
            return Json(res);
        }
    }
}
