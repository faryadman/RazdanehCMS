namespace Project.Application.DTOs.Job.DomainJob
{
    public class CreateDomainJobDTO
    {
        public int? FailConnectionCount { get; set; }
        public int? FailConnectionPercent { get; set; }
        public int? JobPeriodTime { get; set; }
        public int? JobExpireMinuteTime { get; set; }
        public bool IsActiveJob { get; set; } = false;
        public string Email { get; set; }
        public string ApiKey { get; set; }
    }
}
