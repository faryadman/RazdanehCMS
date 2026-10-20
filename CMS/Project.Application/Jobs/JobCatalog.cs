#nullable enable
namespace Project.Application.Jobs;

public sealed record JobDefinition(string Name, string Title, int DefaultMinuteStep, bool EnabledByDefault, bool RequiresReviewAfterFailure);

public static class JobCatalog
{
    public const string Domain = "DomainJob";
    public const string SubDomain = "SubDomainJob";
    public const string DeleteDns = "DeleteDnsJob";
    public const string IpConfig = "IpConfigJob";
    public const string DeleteLogs = "logDeleterJob";
    public const string CheckZones = "CheckZoneId";

    public static IReadOnlyList<JobDefinition> All { get; } = Array.AsReadOnly(new[]
    {
        new JobDefinition(Domain, "تعویض دامنه بر اساس شکست اتصال", 10, false, true),
        new JobDefinition(SubDomain, "تعویض زیردامنه", 10, false, true),
        new JobDefinition(DeleteDns, "پاک‌سازی رکوردهای DNS", 10, false, true),
        new JobDefinition(IpConfig, "تعویض IP سرورها", 10, false, true),
        new JobDefinition(DeleteLogs, "پاک‌سازی لاگ اتصال", 10, true, false),
        new JobDefinition(CheckZones, "تکمیل شناسهٔ zone دامنه‌ها", 45, true, false)
    });

    public static JobDefinition Get(string name) => All.SingleOrDefault(job => job.Name == name)
        ?? throw new Exceptions.BadRequestException("نوع جاب شناخته‌شده نیست.");
}
