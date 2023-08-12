using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Project.Application.DTOs.AppSetting;
using Project.Application.Features.Interfaces;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class AppsController : Controller
    {
        private readonly IAppSettingService _appSettingService;
        private readonly IMemoryCache _memoryCache;
        public AppsController(IAppSettingService appSettingService, IMemoryCache memoryCache)
        {
            _appSettingService = appSettingService;
            _memoryCache = memoryCache;
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
        public async Task<IActionResult> Edit(EditAppSettingDTO input, int id)
        {
            await _appSettingService.Edit(input, id);
            _memoryCache.Remove($"AppSetting_{input.ApiRoute}");
            // ذخیره اطلاعات جدید در حافظه‌ی کش
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };

            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _appSettingService.Detail(id);
            await _appSettingService.Delete(id);
            _memoryCache.Remove($"AppSetting_{data.ApiRoute}");
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> Groups(int id)
        {
            var data = await _appSettingService.GroupsByApp(id);
            return Json(data);
        }
        public async Task<IActionResult> UpdateAppGroups(int id, string groupIds)
        {
            await _appSettingService.UpdateAppGroups(id, groupIds);
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
