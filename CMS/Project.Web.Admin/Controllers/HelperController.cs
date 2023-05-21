using DNTPersianUtils.Core.IranCities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Contracts.Infrastructure;

namespace Project.Web.Admin.Controllers
{
    [Authorize(Policy = Models.ConstantPolicies.DynamicPermission)]
    public class HelperController : Controller
    {
        private readonly IFileStorageService _storageService;

        public HelperController(IFileStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpPost("/upload")]
        public async Task<JsonResult> Upload([FromForm] IFormFile file)
        {
            var data = await _storageService.SaveFile("uploads", file);
            return Json(new { url = data });
        }

        [HttpPost("/drop")]
        public async Task<JsonResult> Drop(string url)
        {
            await _storageService.DeleteFile("uploads", url);
            return Json("");
        }

        [HttpGet("/cities")]
        public JsonResult Cities(string name, int? page = 1)
        {
            var search = string.IsNullOrWhiteSpace(name) ? "" : name;
            var take = 10;
            if (page.HasValue == false || page < 1)
            {
                page = 1;
            }
            var skip = take * (page.Value - 1);

            var res = Iran.Cities
                .OrderBy(x => x.ProvinceName)
                .ThenBy(x => x.CityName)
                .Where(w => w.CityName.Contains(search) || w.ProvinceName.Contains(search))
                .Skip(skip)
                .Take(take)
                .ToList();

            return Json(res);
        }
    }
}
