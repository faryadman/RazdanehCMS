namespace Project.Web.AndroidAppsProject.Jobs;

public static class NativeJobsRegistration
{
    public static IServiceCollection AddNativeJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<NativeJobOptions>().Bind(configuration.GetSection("NativeJobs"))
            .Validate(options => options.PollSeconds is >= 1 and <= 60 && options.ExecutionTimeoutMinutes is >= 1 and <= 1440,
                "تنظیمات زمان انتظار و مهلت اجرای worker معتبر نیست.").ValidateOnStart();
        services.AddHostedService<NativeJobWorker>();
        return services;
    }
}
