using Microsoft.AspNetCore.Mvc;
using Project.Persistence;

namespace Project.Web.AndroidAppsProject.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return Json(_context.AppSettings.ToList());
        }
    }
}
