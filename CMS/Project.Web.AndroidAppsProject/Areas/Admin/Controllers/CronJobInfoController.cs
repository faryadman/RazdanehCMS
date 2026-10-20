using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.CronJobInfo;
using Project.Application.Jobs;
using Project.Domain.Jobs;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "admin")]
public class CronJobInfoController(IJobStore jobs) : Controller
{
    public async Task<IActionResult> Get()
    {
        var dashboard = await jobs.GetDashboardAsync(HttpContext.RequestAborted);
        var job = dashboard.Jobs.Single(job => job.Definition.Name == JobCatalog.DeleteLogs);
        return Json(new CronJobInfoDTO
        {
            Id = 1, Timer = job.Settings.MinuteStep,
            ExecutedCount = job.Schedule.SuccessfulExecutions,
            LastExecutionDate = job.Schedule.LastStartedUtc ?? default,
            LastExecutionDateToString = job.Schedule.LastStartedUtc?.ToString("u") ?? "نامشخص"
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateCronJobInfoDTO input)
    {
        var dashboard = await jobs.GetDashboardAsync(HttpContext.RequestAborted);
        var settings = dashboard.Jobs.Single(job => job.Definition.Name == JobCatalog.DeleteLogs).Settings.ToLegacy();
        settings.JobPeriodTime = input.Timer;
        await jobs.SaveAsync(JobCatalog.DeleteLogs, settings, User.Identity?.Name ?? "admin", HttpContext.RequestAborted);
        return Json(new { status = "1", message = "done successfully" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MassDelete()
    {
        await jobs.RequestRunAsync(JobCatalog.DeleteLogs, HttpContext.RequestAborted);
        return Json(new { status = "1", message = "done successfully" });
    }
}
