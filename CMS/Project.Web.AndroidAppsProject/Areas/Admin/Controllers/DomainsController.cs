using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Domain;
using Project.Application.Features.Interfaces;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class DomainsController : Controller
    {
        private readonly IDomainService _domainService;
        private readonly ICronJobInfoService _cronJobInfoService;

        public DomainsController(IDomainService domainService, ICronJobInfoService cronJobInfoService)
        {
            _domainService = domainService;
            _cronJobInfoService = cronJobInfoService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> List()
        {
            var data = await _domainService.GetAll();
            return Json(data);
        }
        public async Task<IActionResult> CreateDomain(CreateDomainDTO input)
        {
            await _domainService.Create(input);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> DeleteDomain(int id)
        {
            await _domainService.Delete(id);
            return Json(new { status = "1", message = "done successfully" });
        }
    }
}
