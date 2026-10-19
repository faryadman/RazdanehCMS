#nullable enable
using Newtonsoft.Json;
using Project.Application.DTOs.Job.DomainJob;
using Project.Application.Exceptions;
using Project.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

namespace Project.Application.Jobs;

public sealed record JobSettings(bool Enabled, int MinuteStep, int ExpireMinutes, int FailureCount, int FailurePercent)
{
    public static string Fingerprint(Job job) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
        string.Join("\0", job.Id.ToString(CultureInfo.InvariantCulture), job.IsActive.ToString(),
            job.JobPeriodTime?.ToString(CultureInfo.InvariantCulture), job.JobConfig))));

    public static JobSettings Default(string name)
    {
        var definition = JobCatalog.Get(name);
        return new(definition.EnabledByDefault, definition.DefaultMinuteStep, 30, 10, 50);
    }

    public static JobSettings Read(Job job)
    {
        var input = JsonConvert.DeserializeObject<CreateDomainJobDTO>(job.JobConfig ?? "{}")
            ?? throw new BadRequestException("تنظیمات جاب خالی است.");
        input.IsActiveJob = job.IsActive; // The newest database row is authoritative, including a disabled row.
        input.JobPeriodTime = job.JobPeriodTime ?? input.JobPeriodTime;
        return FromInput(job.JobName, input);
    }

    public static JobSettings FromInput(string name, CreateDomainJobDTO input)
    {
        var defaults = Default(name);
        if (input.IsActiveJob && input.JobPeriodTime == null)
            throw new BadRequestException("چرخهٔ دقیقه‌ای جاب فعال باید مشخص باشد.");
        if (input.IsActiveJob && name is JobCatalog.Domain or JobCatalog.IpConfig or JobCatalog.DeleteDns
            && input.JobExpireMinuteTime == null)
            throw new BadRequestException("زمان انقضای جاب فعال باید مشخص باشد.");
        if (input.IsActiveJob && name == JobCatalog.Domain && (input.FailConnectionCount == null || input.FailConnectionPercent == null))
            throw new BadRequestException("آستانهٔ تعداد و درصد شکست جاب دامنه باید مشخص باشد.");
        var settings = new JobSettings(input.IsActiveJob, input.JobPeriodTime ?? defaults.MinuteStep,
            input.JobExpireMinuteTime ?? defaults.ExpireMinutes, input.FailConnectionCount ?? defaults.FailureCount,
            input.FailConnectionPercent ?? defaults.FailurePercent);
        if (settings.MinuteStep is < 1 or > 59)
            throw new BadRequestException("چرخهٔ دقیقه‌ای جاب باید بین ۱ و ۵۹ باشد.");
        if (settings.ExpireMinutes < 0 || settings.FailureCount < 0 || settings.FailurePercent is < 0 or > 100)
            throw new BadRequestException("سن دامنه و تعداد شکست نباید منفی باشند؛ درصد شکست باید بین صفر و صد باشد.");
        if (name == JobCatalog.DeleteDns && settings.Enabled && settings.ExpireMinutes == 0)
            throw new BadRequestException("زمان انقضای رکورد DNS باید بزرگ‌تر از صفر باشد.");
        return settings;
    }

    public CreateDomainJobDTO ToLegacy() => new()
    {
        IsActiveJob = Enabled, JobPeriodTime = MinuteStep, JobExpireMinuteTime = ExpireMinutes,
        FailConnectionCount = FailureCount, FailConnectionPercent = FailurePercent
    };

    public string Serialize() => JsonConvert.SerializeObject(ToLegacy(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
}
