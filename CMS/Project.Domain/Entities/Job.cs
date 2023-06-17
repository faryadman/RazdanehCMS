using Project.Domain.Entities.Base;

namespace Project.Domain.Entities
{
    public class Job : BaseEntity
    {
        public string Email { get; set; }
        public string ApiKey { get; set; }
        public int? JobPeriodTime { get; set; }
        public string JobName { get; set; }
        public string JobConfig { get; set; }
    }
}
