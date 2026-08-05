using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.Application.Contracts.Persistence;
using Project.Persistence.Repositories;

namespace Project.Persistence
{
    public static class PersistenceServicesRegistration
    {
        public static IServiceCollection ConfigurePersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                //options.UseLazyLoadingProxies();
                /*options.EnableSensitiveDataLogging(true);*/
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IAppSettingRepository, AppSettingRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IServerRepository, ServerRepository>();
            services.AddScoped<IServerLogRepository, ServerLogRepository>();
            services.AddScoped<IBlackListRepository, BlackListRepository>();
            services.AddScoped<IApiLogRepository, ApiLogRepository>();
            services.AddScoped<IOperatorIdentificationRepository, OperatorIdentificationRepository>();
            services.AddScoped<ICronJobInfoRepository, CronJobInfoRepository>();
            services.AddScoped<IDomainRepository, DomainRepository>();
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<IIpRepository, IpRepository>();
            services.AddScoped<IIpConfigRepository, IpConfigRepository>();
            return services;
        }
    }
}