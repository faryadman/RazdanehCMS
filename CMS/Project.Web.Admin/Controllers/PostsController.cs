using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Datatable;
using Project.Application.DTOs.Datatable.Base;
using Project.Application.DTOs.Post;
using Project.Application.Extensions;
using Project.Application.Features.Interfaces;
using Project.Application.Responses;
using System.ComponentModel;

namespace Project.Web.Admin.Controllers
{
    [Authorize(Policy = Models.ConstantPolicies.DynamicPermission)]
    [DisplayName("اخبار")]
    public class PostsController : Controller
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [DisplayName("لیست اخبار")]
        public IActionResult Index()
        {
            ViewBag.Title = "لیست اخبار";
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetData(DatatableInput input)
        {
            HttpContext.Request.GetDataFromRequest(out FiltersFromRequestDataTable filters);
            var res = await _postService.Datatable(input, filters);
            return Json(res);
        }

        [DisplayName("ثبت خبر")]
        [HttpGet("[controller]/upsert")]
        public IActionResult Upsert()
        {
            ViewBag.Title = "ثبت خبر";
            return View("upsert");
        }

        [DisplayName("ویرایش خبر")]
        [HttpGet("[controller]/upsert/{id:int}")]
        public async Task<IActionResult> Upsert(int id)
        {
            var res = await _postService.GetToEdit(id);

            if (res == null)
            {
                return NotFound();
            }

            ViewBag.Title = $"ویرایش خبر: {res.Title}";

            return View("upsert", res);
        }

        [HttpPost]
        public async Task<JsonResult> Upsert([FromForm] UpsertPost input)
        {
            if (input.Id.HasValue && input.Id > 0)
            {
                return new Response<PostDTO>(await _postService.Edit(input)).ToJsonResult();
            }
            return new Response<PostDTO>(await _postService.Create(input)).ToJsonResult();
        }

        [DisplayName("حذف خبر")]
        public async Task<JsonResult> Delete(int id)
        {
            await _postService.Delete(id);
            return Response<string>.Succeed();
        }
    }
}
