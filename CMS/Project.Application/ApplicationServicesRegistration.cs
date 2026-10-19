using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Project.Application.Features;
using Project.Application.Features.Interfaces;
using Project.Application.Features.Services;
using Project.Application.Profiles;
using Project.Application.Jobs;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Project.Application
{
    public static class ApplicationServicesRegistration
    {
        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            {
                services.AddSingleton(provider =>
                {
                    var config = new MapperConfiguration(cfg =>
                    {
                        cfg.AddProfile(new MappingProfile());
                    }, provider.GetRequiredService<ILoggerFactory>());

                    return config.CreateMapper();
                });

                services.AddScoped<IAppSettingService, AppSettingService>();
                services.AddScoped<IGroupService, GroupService>();
                services.AddScoped<IServerService, ServerService>();
                services.AddScoped<IServerLogService, ServerLogService>();
                services.AddScoped<IBlackListService, BlackListService>();
                services.AddScoped<IApiLogService, ApiLogService>();
                services.AddScoped<IOperatorIdentificationService, OperatorIdentificationService>();
                services.AddScoped<IDomainService, DomainService>();
                services.AddScoped<IIpService, IpService>();
                services.AddScoped<IIPConfigService, IpConfigService>();
                services.AddScoped<JobExecutionScope>();
                services.AddScoped<IJobHandler, JobHandler>();
                services.TryAddSingleton(TimeProvider.System);
                services.Configure<CloudflareSettings>(configuration.GetSection("CloudflareData"));
                services.AddSingleton<CloudflareApiClient>();
                return services;
            }
        }
    }
}
