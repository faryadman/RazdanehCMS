using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Datatable;
using Project.Application.DTOs.Datatable.Base;
using Project.Application.DTOs.PostCategory;
using Project.Application.Extensions;
using Project.Application.Features.Interfaces;
using Project.Application.Responses;
using System.ComponentModel;

namespace Project.Web.Admin.Controllers
{
    [Authorize(Policy = Models.ConstantPolicies.DynamicPermission)]
    [DisplayName("دسته بندی اخبار")]
    public class PostCategoriesController : Controller
    {
        private readonly IPostCategoryService _postCategoryService;

        public PostCategoriesController(IPostCategoryService postCategoryService)
        {
            _postCategoryService = postCategoryService;
        }

        [DisplayName("لیست دسته بندی اخبار")]
        public IActionResult Index()
        {
            ViewBag.Title = "لیست دسته بندی اخبار";
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetData(DatatableInput input)
        {
            HttpContext.Request.GetDataFromRequest(out FiltersFromRequestDataTable filters);
            var res = await _postCategoryService.Datatable(input, filters);
            return Json(res);
        }

        public async Task<JsonResult> GetAllPaginate(string title, int? page = 1)
        {
            var search = string.IsNullOrWhiteSpace(title) ? "" : title;

            var res = await _postCategoryService.GetAllPaginate(search, page ?? 1);

            return Json(res);
        }

        [HttpPost]
        [DisplayName("ثبت و ویرایش دسته بندی اخبار")]
        public async Task<JsonResult> Upsert(UpsertPostCategory input)
        {
            if (input.Id.HasValue)
            {
                await _postCategoryService.Edit(input);
            }
            else
            {
                await _postCategoryService.Create(input);
            }

            return Response<string>.Succeed();
        }

        [DisplayName("حذف دسته بندی اخبار")]
        public async Task<JsonResult> Delete(int id)
        {
            await _postCategoryService.Delete(id);
            return Response<string>.Succeed();
        }
    }
}
