using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Job;
using Project.Application.Features.Interfaces;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class JobController : Controller
    {

        private readonly IServerService _serverService;
        private readonly IServerLogService _serverLogService;
        private readonly IBlackListService _blackListService;
        private readonly ICronJobInfoService _cronJobInfoService;
        private readonly IDomainService _domainService;

        public JobController(IServerService serverService, IBlackListService blackListService, IServerLogService serverLogService, ICronJobInfoService cronJobInfoService, IDomainService domainService)

        {
            _serverService = serverService;
            _blackListService = blackListService;
            _serverLogService = serverLogService;
            _cronJobInfoService = cronJobInfoService;
            _domainService = domainService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task CreateDomainJob(CreateDomainJobDTO input)
        {
            if (!input.IsActiveJob)
            {
                RecurringJob.RemoveIfExists("Test");
                return;
            }
            RecurringJob.AddOrUpdate("Test", () => Test(), $"*/{input.RunDuringTimeJob} * * * *");
        }

        public void Test()
        {

        }
    }
}
