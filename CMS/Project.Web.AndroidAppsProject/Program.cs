using AutoMapper;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Project.Application;
using Project.Application.Filters;
using Project.Application.Middlewares;
using Project.Domain.Entities;
using Project.Persistence;
using Project.Web.AndroidAppsProject.CronJob;
using Project.Web.AndroidAppsProject.Dapper;
using Serilog;
using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureApplicationServices(new LoggerFactory());
builder.Services.ConfigurePersistenceServices(builder.Configuration);

builder.Services.AddSingleton<ICronJobService, CronJobService>();
builder.Services.AddSingleton<IDapperQueryService, DapperQueryService>();
builder.Services.AddMemoryCache();
builder.Services.AddMemoryCache(options =>
{
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddSingleton<HtmlEncoder>(
     HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.BasicLatin,
                                                           UnicodeRanges.All}));

builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes =
        ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "image/svg+xml" });
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
});


builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = false;
})
.AddRoleManager<RoleManager<IdentityRole>>()
.AddDefaultTokenProviders()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddHangfire(opts =>
{
    opts.UseMemoryStorage();
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "YourAppCookieName";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.LoginPath = "/admin/account";
    // ReturnUrlParameter requires 
    //using Microsoft.AspNetCore.Authentication.Cookies;
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;
});


builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllersWithViews().AddNewtonsoftJson(o =>
{
    o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    //o.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
});

builder.Services
    .AddMvc(option =>
    {
        option.EnableEndpointRouting = false;
        option.Filters.Add(typeof(ModelStateCheckFilter));
    }).AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Add("/{0}.cshtml");
    });
string path = builder.Configuration["Serilog:path"];
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(path, rollingInterval: RollingInterval.Hour)
    .MinimumLevel.Warning()
    .CreateLogger(); builder.Host.UseSerilog();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/page", "?code={0}");

app.UseHttpsRedirection();
app.UseResponseCompression();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    context.Context.Response.Headers.Append("Cache-Control", $"public, max-age={TimeSpan.FromMinutes(10).TotalSeconds}")
});



app.UseHangfireServer();
app.UseHangfireDashboard();

RecurringJob.AddOrUpdate(
    "logDeleterJob",
    () => app.Services.GetService<ICronJobService>()!.ResetServerLog(),
Cron.MinuteInterval(10));

//RecurringJob.AddOrUpdate(
//    "SubDomainJob",
//    () => app.Services.GetService<ICronJobService>()!.ResetServerSubDomain(),
//    Cron.MinuteInterval(30));

//RecurringJob.AddOrUpdate(
//    "DomainJob",
//    () => app.Services.GetService<ICronJobService>()!.ResetServerDomain(),
//    Cron.MinuteInterval(2));

RecurringJob.AddOrUpdate(
    "CheckZoneId",
    () => app.Services.GetService<ICronJobService>()!.CheckZoneId(),
     Cron.MinuteInterval(45));
//RecurringJob.AddOrUpdate(

//"DeleteDnsJob",
//() => app.Services.GetService<ICronJobService>()!.ResetServerDns(),
//Cron.MinuteInterval(10));

app.UseCookiePolicy();

app.UseAuthentication();

app.UseSession();

app.Use(async (context, next) =>
{
    string path = context.Request.Path;
    if (path.EndsWith(".css") || path.EndsWith(".js") || path.EndsWith(".jpg") || path.EndsWith(".jpeg") || path.EndsWith(".png"))
    {
        //Set css and js files to be cached for 10 minutes
        TimeSpan maxAge = new(0, 0, 5, 0);     //    10 minutes
        context.Response.Headers.Append("Cache-Control", "max-age=" + maxAge.TotalSeconds.ToString("0"));
    }
    else
    {
        //Request for views fall here.
        context.Response.Headers.Append("Cache-Control", "no-cache");
        context.Response.Headers.Append("Cache-Control", "private, no-store");
    }
    await next();
});

app.UseRouting();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
});

app.UseMvc(routes =>
{
    routes.MapRoute(
      name: "areas",
      template: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
});

// Seed an initial admin role and user if they do not exist
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var roleExists = roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult();
        if (!roleExists)
        {
            roleManager.CreateAsync(new IdentityRole("admin")).GetAwaiter().GetResult();
        }

        var adminUserName = "admin";
        var adminUser = userManager.FindByNameAsync(adminUserName).GetAwaiter().GetResult();
        if (adminUser == null)
        {
            var user = new User
            {
                UserName = adminUserName,
                NormalizedUserName = adminUserName.ToUpper(),
                Email = "admin@local",
                NormalizedEmail = "admin@local".ToUpper(),
                EmailConfirmed = true,
                LockoutEnabled = false
            };

            var createResult = userManager.CreateAsync(user, "Admin@123").GetAwaiter().GetResult();
            if (createResult.Succeeded)
            {
                userManager.AddToRoleAsync(user, "admin").GetAwaiter().GetResult();
            }
        }
    }
    catch (Exception ex)
    {
        // Swallow exceptions during seeding to avoid stopping the app; consider logging in real scenarios
    }
}

app.Run();
