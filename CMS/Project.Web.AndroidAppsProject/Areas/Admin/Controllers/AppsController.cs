using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.AppSetting;
using Project.Application.Features.Interfaces;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize( Roles ="admin")]
    public class AppsController : Controller
    {
        private readonly IAppSettingService _appSettingService;

        public AppsController(IAppSettingService appSettingService)
        {
            _appSettingService = appSettingService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> List()
        {
            var data = await _appSettingService.GetAll();
            return Json(data);
        }
        public async Task<IActionResult> Create(CreateAppSettingDTO input)
        {
            await _appSettingService.Create(input);
            return Json(new { status = "1", message = "done successfully" });
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _appSettingService.Detail(id);
            return View(data);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(EditAppSettingDTO input,int id)
        {
            await _appSettingService.Edit(input,id);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> Delete(int id)
        {
            await _appSettingService.Delete(id);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> Groups(int id)
        {
            var data = await _appSettingService.GroupsByApp(id);
            return Json(data);
        }
        public async Task<IActionResult> UpdateAppGroups(int id,string groupIds)
        {
            await _appSettingService.UpdateAppGroups(id,groupIds);
            return Json(new { status = "1", message = "done successfully" });
        }

        public async Task<IActionResult> MassDelete(string ids)
        {
            foreach (var item in ids.Split("_"))
            {
                await _appSettingService.Delete(int.Parse(item));
            }
            return Json(new { status = "1", message = "done successfully" });
        }
    }
}
