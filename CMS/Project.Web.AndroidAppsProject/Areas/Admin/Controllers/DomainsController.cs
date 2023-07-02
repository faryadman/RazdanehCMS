using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Domain;
using Project.Application.Features.Interfaces;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class DomainsController : Controller
    {
        private readonly IDomainService _domainService;
        private readonly ICronJobInfoService _cronJobInfoService;
        private readonly IWebHostEnvironment _env;

        public DomainsController(IDomainService domainService, ICronJobInfoService cronJobInfoService, IWebHostEnvironment env)
        {
            _domainService = domainService;
            _cronJobInfoService = cronJobInfoService;
            _env = env;
        }
        public IActionResult Index()
        {

            return View();
        }
        public async Task<IActionResult> List(int filter)
        {
            var data = await _domainService.GetByFilter(filter);
            return Json(data);
        }
        public async Task<IActionResult> ListInactive(int filter = 0)
        {
            var data = await _domainService.GetByFilter(filter);
            return Json(data);
        }
        public async Task<IActionResult> CreateDomain(CreateDomainDTO input)
        {
            await _domainService.Create(input);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> DeleteDomain(int id)
        {
            await _domainService.Delete(id);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> DeleteInactiveDomain()
        {
            await _domainService.DeleteInactiveDomain();
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> MassDelete(string ids)
        {
            foreach (var item in ids.Split("_"))
            {
                await DeleteDomain(int.Parse(item));
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
                var filePath = Path.Combine(_env.WebRootPath, "UploadedFiles", _FileName);
                await using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }
                List<string> lines = new List<string>();
                // واکشی خط‌های موجود در فایل متنی
                if (System.IO.File.Exists(filePath))
                {
                    lines = System.IO.File.ReadAllLines(filePath).ToList();
                }
                // جدا سازی داده‌ها از هر خط با استفاده از کاراکتر اسپیس (Space)
                foreach (string line in lines)
                {
                    // ذخیره داده‌های جدا ساخته شده در لیستی یا در دیتابیس 
                    await _domainService.Create(new CreateDomainDTO() { DomainName = line, FileName = _FileName });
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
