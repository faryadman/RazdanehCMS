using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Datatable;
using Project.Application.DTOs.Datatable.Base;
using Project.Application.DTOs.SliderImage;
using Project.Application.Extensions;
using Project.Application.Features.Interfaces;
using Project.Application.Responses;
using System.ComponentModel;

namespace Project.Web.Admin.Controllers
{
    [Authorize(Policy = Models.ConstantPolicies.DynamicPermission)]
    [DisplayName("تصاویر اسلایدر")]
    public class SliderImagesController : Controller
    {
        private readonly ISliderImageService _sliderImageService;

        public SliderImagesController(ISliderImageService sliderImageService)
        {
            _sliderImageService = sliderImageService;
        }

        [DisplayName("لیست تصاویر اسلایدر")]
        public IActionResult Index()
        {
            ViewBag.Title = "لیست تصاویر اسلایدر";
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetData(DatatableInput input)
        {
            HttpContext.Request.GetDataFromRequest(out FiltersFromRequestDataTable filters);
            var res = await _sliderImageService.Datatable(input, filters);
            return Json(res);
        }

        [DisplayName("ثبت تصویر اسلایدر")]
        [HttpGet("[controller]/upsert")]
        public IActionResult Upsert()
        {
            ViewBag.Title = "ثبت تصویر اسلایدر";
            return View("upsert");
        }

        [DisplayName("ویرایش تصویر اسلایدر")]
        [HttpGet("[controller]/upsert/{id:int}")]
        public async Task<IActionResult> Upsert(int id)
        {
            var res = await _sliderImageService.GetToEdit(id);

            if (res == null)
            {
                return NotFound();
            }

            ViewBag.Title = "ویرایش تصویر اسلایدر";

            return View("upsert", res);
        }

        [HttpPost]
        public async Task<JsonResult> Upsert([FromForm] UpsertSliderImage input)
        {
            if (input.Id.HasValue && input.Id > 0)
            {
                return new Response<SliderImageDTO>(await _sliderImageService.Edit(input)).ToJsonResult();
            }
            return new Response<SliderImageDTO>(await _sliderImageService.Create(input)).ToJsonResult();
        }

        [DisplayName("حذف تصویر اسلایدر")]
        public async Task<JsonResult> Delete(int id)
        {
            await _sliderImageService.Delete(id);
            return Response<string>.Succeed();
        }
    }
}
