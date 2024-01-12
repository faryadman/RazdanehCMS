using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Project.Infrastructure
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection ConfigureInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            //services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            //services.AddTransient<IEmailSender, EmailSender>();

            //services.Configure<SmsSettings>(configuration.GetSection("SmsSettings"));
            //services.AddTransient<ISmsSender, SmsSender>();

            return services;
        }
    }
}
