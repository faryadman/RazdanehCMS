using System.Diagnostics;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Project.Application.DTOs.Server;
using Project.Application.Exceptions;
using Project.Application.Features.Interfaces;
using Project.Application.Middlewares;
using Project.Web.AndroidAppsProject.Configuration;
using Project.Web.AndroidAppsProject.Controllers;

namespace Project.LegacyApi.Tests;

public sealed class LegacyApiHost : IDisposable
{
    private readonly IHost host;
    public IAppSettingService AppSettings { get; } = Substitute.For<IAppSettingService>();
    public IServerService Servers { get; } = Substitute.For<IServerService>();
    public IServerLogService ServerLogs { get; } = Substitute.For<IServerLogService>();
    public IIpService Ips { get; } = Substitute.For<IIpService>();
    public IBlackListService BlackLists { get; } = Substitute.For<IBlackListService>();
    public HttpClient Client { get; }
    public IServiceProvider Services => host.Services;

    public LegacyApiHost()
    {
        AppSettings.DetailByApiRoute(Arg.Any<string>()).Returns(call => call.Arg<string>() switch
        {
            "missing" => Task.FromException<Project.Application.DTOs.AppSetting.AppSettingDTO>(new NotFoundException("برنامه یافت نشد")),
            "empty" => Task.FromResult<Project.Application.DTOs.AppSetting.AppSettingDTO>(null!),
            "broken" => Task.FromException<Project.Application.DTOs.AppSetting.AppSettingDTO>(new InvalidOperationException("خطای ساختگی", new Exception("جزئیات ساختگی"))),
            _ => Task.FromResult(SyntheticData.AppSetting())
        });
        Servers.GetByApp(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => Task.FromResult(SyntheticData.Server(call.ArgAt<bool>(1))));
        Servers.Detail(Arg.Any<int>()).Returns(Task.FromResult(SyntheticData.Server()));
        Servers.GetWithFilter(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<bool>(), Arg.Any<int>())
            .Returns(call => Task.FromResult(new List<ServerDTO> { SyntheticData.Server(call.ArgAt<bool>(2)) }));
        ServerLogs.ListByServer(Arg.Any<int>()).Returns(Task.FromResult(SyntheticData.Logs()));
        Ips.List().Returns(Task.FromResult(SyntheticData.Ips()));
        BlackLists.List(Arg.Any<int?>()).Returns(Task.FromResult(SyntheticData.BlackList()));

        // Entry point, Identity seeding, Hangfire, SQL and DNS are deliberately not registered.
        host = new HostBuilder().ConfigureWebHost(builder => builder.UseEnvironment("Development").UseTestServer()
            .ConfigureServices(services =>
            {
                services.AddSingleton(AppSettings);
                services.AddSingleton(Servers);
                services.AddSingleton(ServerLogs);
                services.AddSingleton(Ips);
                services.AddSingleton(BlackLists);
                services.AddMemoryCache();
                services.AddLegacyAndroidMvc().AddApplicationPart(typeof(ApiController).Assembly);
            })
            .Configure(app =>
            {
                app.Use((context, next) =>
                {
                    Activity.Current = null;
                    context.TraceIdentifier = "legacy-contract-request";
                    context.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.20");
                    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                    CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
                    return next(context);
                });
                app.UseMiddleware<ExceptionMiddleware>();
                app.UseMvc(routes =>
                {
                    routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
                    routes.MapRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                });
            })).Start();
        Client = host.GetTestClient();
        Client.DefaultRequestHeaders.UserAgent.ParseAdd("LegacyContractTests/1.0");
    }

    public void Dispose()
    {
        Client.Dispose();
        host.Dispose();
    }
}
