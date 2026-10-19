using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Job.DomainJob;
using Project.Application.Jobs;
using Project.Application.Exceptions;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "admin")]
public class JobController(IJobStore jobs) : Controller
{
    public async Task<IActionResult> Index() => View(await jobs.GetDashboardAsync(HttpContext.RequestAborted));
    public Task<IActionResult> GetDeleteDnsJobData() => ReadSettings(JobCatalog.DeleteDns);
    public Task<IActionResult> GetJobSubdomainData() => ReadSettings(JobCatalog.SubDomain);
    public Task<IActionResult> GetJobDomainData() => ReadSettings(JobCatalog.Domain);
    public Task<IActionResult> GetJobIpConfigData() => ReadSettings(JobCatalog.IpConfig);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CreateDomainJob(CreateDomainJobDTO input) => SaveSettings(JobCatalog.Domain, input);
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CreateDeleteDnsJobJob(CreateDomainJobDTO input) => SaveSettings(JobCatalog.DeleteDns, input);
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CreateSubDomainJob(CreateDomainJobDTO input) => SaveSettings(JobCatalog.SubDomain, input);
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CreateIpConfigJob(CreateDomainJobDTO input) => SaveSettings(JobCatalog.IpConfig, input);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task CheckDomainJob() => await jobs.RequestRunAsync(JobCatalog.Domain, HttpContext.RequestAborted);
    [HttpPost, ValidateAntiForgeryToken]
    public async Task CheckSubDomainJob() => await jobs.RequestRunAsync(JobCatalog.SubDomain, HttpContext.RequestAborted);
    [HttpPost, ValidateAntiForgeryToken]
    public async Task CheckDeleteDnsDomainJob() => await jobs.RequestRunAsync(JobCatalog.DeleteDns, HttpContext.RequestAborted);
    [HttpPost, ValidateAntiForgeryToken]
    public async Task CheckIpConfigJob() => await jobs.RequestRunAsync(JobCatalog.IpConfig, HttpContext.RequestAborted);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Save(string jobName, CreateDomainJobDTO input) => SaveSettings(jobName, input);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Run(string jobName)
    {
        await jobs.RequestRunAsync(jobName, HttpContext.RequestAborted);
        return Json(new { status = "1", message = "درخواست اجرا در دیتابیس ثبت شد." });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(Guid executionId)
    {
        await jobs.ReviewAsync(executionId, User.Identity?.Name ?? "admin", HttpContext.RequestAborted);
        return Json(new { status = "1", message = "بررسی ثبت شد؛ زمان‌بند می‌تواند ادامه دهد." });
    }

    private async Task<IActionResult> ReadSettings(string name)
    {
        var dashboard = await jobs.GetDashboardAsync(HttpContext.RequestAborted);
        return Json(dashboard.Jobs.Single(job => job.Definition.Name == name).Settings.ToLegacy());
    }

    private async Task<IActionResult> SaveSettings(string name, CreateDomainJobDTO input)
    {
        if (!ModelState.IsValid) throw new BadRequestException("مقادیر فرم جاب معتبر نیستند.");
        await jobs.SaveAsync(name, input, User.Identity?.Name ?? "admin", HttpContext.RequestAborted);
        return Json(new { status = "1", message = "done successfully" });
    }
}
