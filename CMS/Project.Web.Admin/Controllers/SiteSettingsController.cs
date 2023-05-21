using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Datatable.Base;
using Project.Application.DTOs.SiteSetting;
using Project.Application.Extensions;
using Project.Application.Features.Interfaces;
using Project.Application.Responses;
using System.ComponentModel;

namespace Project.Web.Admin.Controllers
{
    [Authorize(Policy = Models.ConstantPolicies.DynamicPermission)]
    [DisplayName("تنظیمات سایت")]
    public class SiteSettingsController : Controller
    {
        private readonly ISiteSettingService _siteSettingService;

        public SiteSettingsController(ISiteSettingService siteSettingService)
        {
            _siteSettingService = siteSettingService;
        }

        [DisplayName("لیست تنظیمات سایت")]
        public IActionResult Index()
        {
            ViewBag.Title = "لیست تنظیمات سایت";
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetData(DatatableInput input)
        {
            HttpContext.Request.GetDataFromRequest(out FiltersFromRequestDataTable filters);
            var res = await _siteSettingService.Datatable(input, filters);
            return Json(res);
        }

        [HttpPost]
        [DisplayName("ثبت و ویرایش تنظیمات سایت")]
        public async Task<JsonResult> Upsert(UpsertSiteSetting input)
        {
            if (input.Id.HasValue)
            {
                await _siteSettingService.Edit(input);
            }
            else
            {
                await _siteSettingService.Create(input);
            }

            return Response<string>.Succeed();
        }

        [DisplayName("حذف تنظیمات سایت")]
        public async Task<JsonResult> Delete(int id)
        {
            await _siteSettingService.Delete(id);
            return Response<string>.Succeed();
        }
    }
}
