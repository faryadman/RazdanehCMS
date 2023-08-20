using Microsoft.AspNetCore.Mvc;
using Project.Application.Features.Interfaces;
using Project.Application.Responses;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class IpController : Controller
    {
        private readonly IIpService _ipService;

        public IpController(IIpService ipService)
        {
            _ipService = ipService;
        }
        public IActionResult Index()
        {
            return View();
        }
        //TODO: dont need int id! delete it.
        public async Task<IActionResult> DeleteIp(int id)
        {
            await _ipService.Delete();
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }
        public async Task<IActionResult> MassDelete(string ids)
        {
            foreach (var item in ids.Split("_"))
            {
                await _ipService.Delete(int.Parse(item));
            }
            return Json(new { status = "1", message = "done successfully" });
        }
    }
}
