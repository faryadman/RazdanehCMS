namespace Project.Application.DTOs.Job
{
    public class JobDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string ApiKey { get; set; }
        public string JobName { get; set; }
        public string JobConfig { get; set; }
        public bool IsActive { get; set; }
        public int? JobPeriodTime { get; set; }
        public int? JobExpireMinuteTime { get; set; }
    }
}
