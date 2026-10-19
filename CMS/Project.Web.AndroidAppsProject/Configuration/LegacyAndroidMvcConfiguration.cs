using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Project.Application.Filters;

namespace Project.Web.AndroidAppsProject.Configuration;

public static class LegacyAndroidMvcConfiguration
{
    public static IMvcBuilder AddLegacyAndroidMvc(this IServiceCollection services)
    {
        services.AddRouting(options => options.LowercaseUrls = true);

        services.AddControllersWithViews().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        });

        return services.AddMvc(options =>
        {
            options.EnableEndpointRouting = false;
            options.Filters.Add(typeof(ModelStateCheckFilter));
        }).AddRazorOptions(options =>
        {
            options.ViewLocationFormats.Add("/{0}.cshtml");
        });
    }
}
