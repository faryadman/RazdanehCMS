using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Project.Application.Features.Interfaces;
using Project.Application.Features.Services;
using Project.Application.Profiles;
using System.Reflection;

namespace Project.Application
{
    public static class ApplicationServicesRegistration
    {
        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddSingleton(provider => new MapperConfiguration(config =>
            {
                config.AddProfile(new MappingProfile());
            }).CreateMapper());

            services.AddScoped<IAppSettingService, AppSettingService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IServerService, ServerService>();
            services.AddScoped<IServerLogService, ServerLogService>();
            services.AddScoped<IBlackListService, BlackListService>();
            services.AddScoped<IApiLogService, ApiLogService>();
            services.AddScoped<IOperatorIdentificationService, OperatorIdentificationService>();
            services.AddScoped<ICronJobInfoService, CronJobInfoService>();
            services.AddScoped<IDomainService, DomainService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IIpService, IpService>();
            services.AddScoped<IIPConfigService, IpConfigService>();

            return services;
        }
    }
}
