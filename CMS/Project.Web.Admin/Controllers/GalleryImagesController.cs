using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Datatable;
using Project.Application.DTOs.Datatable.Base;
using Project.Application.DTOs.GalleryImage;
using Project.Application.Extensions;
using Project.Application.Features.Interfaces;
using Project.Application.Responses;
using System.ComponentModel;

namespace Project.Web.Admin.Controllers
{
    [Authorize(Policy = Models.ConstantPolicies.DynamicPermission)]
    [DisplayName("تصاویر گالری")]
    public class GalleryImagesController : Controller
    {
        private readonly IGalleryImageService _galleryImageService;

        public GalleryImagesController(IGalleryImageService galleryImageService)
        {
            _galleryImageService = galleryImageService;
        }

        [DisplayName("لیست تصاویر گالری")]
        public IActionResult Index()
        {
            ViewBag.Title = "لیست تصاویر گالری";
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetData(DatatableInput input)
        {
            HttpContext.Request.GetDataFromRequest(out FiltersFromRequestDataTable filters);
            var res = await _galleryImageService.Datatable(input, filters);
            return Json(res);
        }

        [DisplayName("ثبت تصویر گالری")]
        [HttpGet("[controller]/upsert")]
        public IActionResult Upsert()
        {
            ViewBag.Title = "ثبت تصویر گالری";
            return View("upsert");
        }

        [DisplayName("ویرایش تصویر گالری")]
        [HttpGet("[controller]/upsert/{id:int}")]
        public async Task<IActionResult> Upsert(int id)
        {
            var res = await _galleryImageService.GetToEdit(id);

            if (res == null)
            {
                return NotFound();
            }

            ViewBag.Title = $"ویرایش تصویر گالری: {res.Title}";

            return View("upsert", res);
        }

        [HttpPost]
        public async Task<JsonResult> Upsert([FromForm] UpsertGalleryImage input)
        {
            if (input.Id.HasValue && input.Id > 0)
            {
                return new Response<GalleryImageDTO>(await _galleryImageService.Edit(input)).ToJsonResult();
            }
            return new Response<GalleryImageDTO>(await _galleryImageService.Create(input)).ToJsonResult();
        }

        [DisplayName("حذف تصویر گالری")]
        public async Task<JsonResult> Delete(int id)
        {
            await _galleryImageService.Delete(id);
            return Response<string>.Succeed();
        }
    }
}
