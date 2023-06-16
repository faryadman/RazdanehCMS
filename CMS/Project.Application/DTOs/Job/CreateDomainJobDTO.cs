namespace Project.Application.DTOs.Job
{
    public class CreateDomainJobDTO
    {
        public int? RunDuringTimeJob { get; set; }
        public int? FailConnectionCount { get; set; }
        public int? FailConnectionPercent { get; set; }
        public int? MinuteCheckTime { get; set; }
        public bool IsActiveJob { get; set; }

    }
}
