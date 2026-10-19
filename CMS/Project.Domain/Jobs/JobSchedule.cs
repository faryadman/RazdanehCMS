#nullable enable
namespace Project.Domain.Jobs;

public sealed class JobSchedule
{
    public string JobName { get; set; } = "";
    public int ConfigurationId { get; set; }
    public string ConfigurationHash { get; set; } = "";
    public bool Enabled { get; set; }
    public int MinuteStep { get; set; }
    public DateTime? NextRunUtc { get; set; }
    public bool RunRequested { get; set; }
    public DateTime? RequestedUtc { get; set; }
    public DateTime? LastStartedUtc { get; set; }
    public DateTime? LastCompletedUtc { get; set; }
    public string? LastStatus { get; set; }
    public string? ValidationError { get; set; }
    public int SuccessfulExecutions { get; set; }
}
