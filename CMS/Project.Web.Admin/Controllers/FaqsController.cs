using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Datatable.Base;
using Project.Application.DTOs.Faq;
using Project.Application.Extensions;
using Project.Application.Features.Interfaces;
using Project.Application.Responses;
using System.ComponentModel;

namespace Project.Web.Admin.Controllers
{
    [Authorize(Policy = Models.ConstantPolicies.DynamicPermission)]
    [DisplayName("سوالات متداول")]
    public class FaqsController : Controller
    {
        private readonly IFaqService _faqService;

        public FaqsController(IFaqService faqService)
        {
            _faqService = faqService;
        }

        [DisplayName("لیست سوالات متداول")]
        public IActionResult Index()
        {
            ViewBag.Title = "لیست سوالات متداول";
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetData(DatatableInput input)
        {
            HttpContext.Request.GetDataFromRequest(out FiltersFromRequestDataTable filters);
            var res = await _faqService.Datatable(input, filters);
            return Json(res);
        }

        [HttpPost]
        [DisplayName("ثبت و ویرایش سوال متداول")]
        public async Task<JsonResult> Upsert(UpsertFaq input)
        {
            if (input.Id.HasValue)
            {
                await _faqService.Edit(input);
            }
            else
            {
                await _faqService.Create(input);
            }

            return Response<string>.Succeed();
        }

        [DisplayName("حذف سوال متداول")]
        public async Task<JsonResult> Delete(int id)
        {
            await _faqService.Delete(id);
            return Response<string>.Succeed();
        }
    }
}
