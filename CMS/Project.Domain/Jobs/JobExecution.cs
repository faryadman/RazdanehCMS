#nullable enable
namespace Project.Domain.Jobs;

public static class JobExecutionStatus
{
    public const string Running = "Running";
    public const string Succeeded = "Succeeded";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
    public const string Interrupted = "Interrupted";
}

public sealed class JobExecution
{
    public Guid Id { get; set; }
    public string JobName { get; set; } = "";
    public int ConfigurationId { get; set; }
    public string SettingsJson { get; set; } = "";
    public string WorkerId { get; set; } = "";
    public string Trigger { get; set; } = "";
    public DateTime ScheduledForUtc { get; set; }
    public DateTime StartedUtc { get; set; }
    public DateTime? CompletedUtc { get; set; }
    public string Status { get; set; } = JobExecutionStatus.Running;
    public string? ErrorCode { get; set; }
    public bool NeedsReview { get; set; }
    public DateTime? ReviewedUtc { get; set; }
    public string? ReviewedBy { get; set; }
}
