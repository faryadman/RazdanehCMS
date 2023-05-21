using Microsoft.AspNetCore.Mvc;

namespace Project.Web.Admin.Controllers
{
    public class ErrorController : Controller
    {
        //[HttpGet("/error")]
        public IActionResult Page(int? code = null)
        {
            ViewBag.Code = "خطا";
            if (code.HasValue)
            {
                ViewBag.Code = code.Value.ToString();
            }

            return View();
        }

    }
}
