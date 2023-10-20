using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Domain;
using Project.Application.DTOs.IpConfig;
using Project.Application.Features.Interfaces;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class IPConfigController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IIPConfigService _ipConfigService;

        public IPConfigController(IWebHostEnvironment env, IIPConfigService ipConfigService)
        {
            _env = env;
            _ipConfigService = ipConfigService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> List()
        {
            var data = await _ipConfigService.GetAll();
            return Json(data);
        }
        public async Task<IActionResult> ListInactive()
        {
            //var data = await _domainService.ListInactiveDomain();
            //return Json(data);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> Create(CreateDomainDTO input)
        {
            //await _domainService.Create(input);
            //return Json(new { status = "1", message = "done successfully" });
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> Delete(int id)
        {
            await _ipConfigService.Delete(id);
            return Json(new { status = "1", message = "done successfully" });
        }
        public Task<IActionResult> DeleteInactiveDomain()
        {
            ////TODO: Refactor into service
            //var list = _domainService.Remove();
            return Task.FromResult<IActionResult>(Json(new { status = "1", message = "done successfully" }));
        }
        public async Task<IActionResult> MassDelete(string ids)
        {
            foreach (var item in ids.Split("_"))
            {
                await Delete(int.Parse(item));
            }
            return Json(new { status = "1", message = "done successfully" });
        }
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file.Length <= 0) return RedirectToAction("Index");
                var _FileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(_env.WebRootPath, "IP_Files", _FileName);
                await using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }
                List<string> lines = new();
                // واکشی خط‌های موجود در فایل متنی
                if (System.IO.File.Exists(filePath))
                {
                    lines = System.IO.File.ReadAllLines(filePath).ToList();
                }
                // جدا سازی داده‌ها از هر خط با استفاده از کاراکتر اسپیس (Space)
                foreach (string line in lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        // ذخیره داده‌های جدا ساخته شده در لیستی یا در دیتابیس 
                        await _ipConfigService.Create(new IPConfigDTO() { Ip = line, FileName = _FileName });
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Json(new { status = "2", message = ex.Message });
            }
        }
    }
}
