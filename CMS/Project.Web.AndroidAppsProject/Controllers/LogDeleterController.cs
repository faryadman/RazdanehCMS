using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.Jobs;

namespace Project.Web.AndroidAppsProject.Controllers;

[Authorize(Roles = "admin")]
public class LogDeleterController(IJobStore jobs) : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index()
    {
        await jobs.RequestRunAsync(JobCatalog.DeleteLogs, HttpContext.RequestAborted);
        return Ok(true);
    }
}
