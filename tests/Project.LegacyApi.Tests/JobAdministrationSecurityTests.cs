using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Project.Application.DTOs.Job.DomainJob;
using Project.Application.Jobs;
using Project.Application.Middlewares;
using Project.Domain.Jobs;
using Project.Web.AndroidAppsProject.Areas.Admin.Controllers;
using Project.Web.AndroidAppsProject.Configuration;
using Project.Web.AndroidAppsProject.Controllers;
using Xunit;

namespace Project.LegacyApi.Tests;

public sealed class JobAdministrationSecurityTests
{
    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("user", HttpStatusCode.Forbidden)]
    public async Task Job_administration_is_inaccessible_without_the_admin_role(string? role, HttpStatusCode status)
    {
        using var host = CreateHost(Substitute.For<IJobStore>());
        using var client = host.GetTestClient();
        if (role != null) client.DefaultRequestHeaders.Add("X-Test-Role", role);
        using var response = await client.GetAsync("/Admin/Job");
        Assert.Equal(status, response.StatusCode);
    }

    [Fact]
    public async Task Admin_mutation_without_an_antiforgery_token_never_reaches_the_store()
    {
        var store = Substitute.For<IJobStore>();
        using var host = CreateHost(store);
        using var client = host.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "admin");
        using var body = new FormUrlEncodedContent(new Dictionary<string, string> { ["jobName"] = JobCatalog.Domain });
        using var response = await client.PostAsync("/Admin/Job/Run", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await store.DidNotReceive().RequestRunAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rendered_admin_form_can_save_settings_and_queue_a_manual_run_with_a_valid_token()
    {
        var store = Substitute.For<IJobStore>();
        store.GetDashboardAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new JobDashboard(
            JobCatalog.All.Select(job => new JobOverview(job, JobSettings.Default(job.Name),
                new JobSchedule { JobName = job.Name, Enabled = true })).ToArray(), Array.Empty<JobExecution>(), false)));
        using var host = CreateHost(store);
        using var client = host.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "admin");
        using var page = await client.GetAsync("/Admin/Job");
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var html = await page.Content.ReadAsStringAsync();
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(token);
        Assert.Contains("/Admin/Job/Save", html);
        client.DefaultRequestHeaders.Add("Cookie", page.Headers.GetValues("Set-Cookie").First().Split(';')[0]);
        using var settings = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token, ["jobName"] = JobCatalog.Domain,
            ["IsActiveJob"] = "true", ["JobPeriodTime"] = "15", ["JobExpireMinuteTime"] = "30",
            ["FailConnectionCount"] = "10", ["FailConnectionPercent"] = "50"
        });
        using var saved = await client.PostAsync("/Admin/Job/Save", settings);
        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        await store.Received(1).SaveAsync(JobCatalog.Domain, Arg.Is<CreateDomainJobDTO>(input => input.IsActiveJob
            && input.JobPeriodTime == 15 && input.JobExpireMinuteTime == 30), "admin", Arg.Any<CancellationToken>());
        using var request = new FormUrlEncodedContent(new Dictionary<string, string>
            { ["__RequestVerificationToken"] = token, ["jobName"] = JobCatalog.Domain });
        using var queued = await client.PostAsync("/Admin/Job/Run", request);
        Assert.Equal(HttpStatusCode.OK, queued.StatusCode);
        await store.Received(1).RequestRunAsync(JobCatalog.Domain, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Every_job_mutation_is_post_only_and_requires_admin_and_antiforgery()
    {
        var controllers = new[] { typeof(JobController), typeof(CronJobInfoController), typeof(LogDeleterController) };
        foreach (var controller in controllers)
        {
            Assert.Equal("admin", Assert.Single(controller.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>()).Roles);
            foreach (var action in controller.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly))
            {
                if (controller == typeof(JobController) && (action.Name == "Index" || action.Name.StartsWith("Get", StringComparison.Ordinal))) continue;
                if (controller == typeof(CronJobInfoController) && action.Name == "Get") continue;
                Assert.True(action.IsDefined(typeof(HttpPostAttribute), false), action.Name);
                Assert.True(action.IsDefined(typeof(ValidateAntiForgeryTokenAttribute), false), action.Name);
            }
        }
        Assert.Equal(new[] { typeof(Guid) }, typeof(JobController).GetMethod("Review")!.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(new[] { typeof(string), typeof(CreateDomainJobDTO) }, typeof(JobController).GetMethod("Save")!.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(new[] { typeof(string) }, typeof(JobController).GetMethod("Run")!.GetParameters().Select(parameter => parameter.ParameterType));
    }

    private static IHost CreateHost(IJobStore store) => new HostBuilder().ConfigureWebHost(builder => builder.UseTestServer()
        .ConfigureServices(services =>
        {
            services.AddSingleton(store);
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
            services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("test", _ => { });
            services.AddAuthorization();
            services.AddLegacyAndroidMvc().AddApplicationPart(typeof(JobController).Assembly);
        }).Configure(app =>
        {
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseAuthentication();
            app.UseMvc(routes => routes.MapRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}"));
        })).Start();

    private sealed class TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var role = Request.Headers["X-Test-Role"].ToString();
            if (string.IsNullOrEmpty(role)) return Task.FromResult(AuthenticateResult.NoResult());
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, role), new Claim(ClaimTypes.NameIdentifier, role), new Claim(ClaimTypes.Role, role) }, "test");
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "test")));
        }
    }
}
