namespace Project.Application.DTOs.Job
{
    public class HangfireDTO
    {
        public string Id { get; set; }
        public string Cron { get; set; }
        public string LastExecTime { get; set; }
        public string NextExecTime { get; set; }
        public string CreatedAt { get; set; }
    }
}
