using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.OperatorIdentification;
using Project.Application.Features.Interfaces;
using System.Data;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class IspController : Controller
    {
        private readonly IOperatorIdentificationService _operatorIdentificationService;

        public IspController(IOperatorIdentificationService operatorIdentificationService)
        {
            _operatorIdentificationService = operatorIdentificationService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> List()
        {
            var data = await _operatorIdentificationService.GetAll(true);
            return Json(data);
        }
        public async Task<IActionResult> Create(CreateOperatorIdentificationDTO input)
        {
            input.IsIsp = true;
            await _operatorIdentificationService.Create(input);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> Delete(int id)
        {
            await _operatorIdentificationService.Delete(id);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> MassDelete(string ids)
        {
            foreach (var item in ids.Split("_"))
            {
                await _operatorIdentificationService.Delete(int.Parse(item));
            }
            return Json(new { status = "1", message = "done successfully" });
        }
    }
}
