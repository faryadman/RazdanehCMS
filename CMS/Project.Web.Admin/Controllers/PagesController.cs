using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Datatable;
using Project.Application.DTOs.Datatable.Base;
using Project.Application.DTOs.Page;
using Project.Application.Extensions;
using Project.Application.Features.Interfaces;
using Project.Application.Responses;
using System.ComponentModel;

namespace Project.Web.Admin.Controllers
{
    [Authorize(Policy = Models.ConstantPolicies.DynamicPermission)]
    [DisplayName("صفحات")]
    public class PagesController : Controller
    {
        private readonly IPageService _pageService;

        public PagesController(IPageService pageService)
        {
            _pageService = pageService;
        }

        [DisplayName("لیست صفحات")]
        public IActionResult Index()
        {
            ViewBag.Title = "لیست صفحات";
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetData(DatatableInput input)
        {
            HttpContext.Request.GetDataFromRequest(out FiltersFromRequestDataTable filters);
            var res = await _pageService.Datatable(input, filters);
            return Json(res);
        }

        [DisplayName("ثبت صفحه")]
        [HttpGet("[controller]/upsert")]
        public IActionResult Upsert()
        {
            ViewBag.Title = "ثبت صفحه";
            return View("upsert");
        }

        [DisplayName("ویرایش صفحه")]
        [HttpGet("[controller]/upsert/{id:int}")]
        public async Task<IActionResult> Upsert(int id)
        {
            var res = await _pageService.GetToEdit(id);

            if (res == null)
            {
                return NotFound();
            }

            ViewBag.Title = $"ویرایش صفحه: {res.Title}";

            return View("upsert", res);
        }

        [HttpPost]
        public async Task<JsonResult> Upsert([FromForm] UpsertPage input)
        {
            if (input.Id.HasValue && input.Id > 0)
            {
                return new Response<PageDTO>(await _pageService.Edit(input)).ToJsonResult();
            }
            return new Response<PageDTO>(await _pageService.Create(input)).ToJsonResult();
        }

        [DisplayName("حذف صفحه")]
        public async Task<JsonResult> Delete(int id)
        {
            await _pageService.Delete(id);
            return Response<string>.Succeed();
        }
    }
}
