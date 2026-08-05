using Project.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Entities
{
    public class Job : BaseEntity
    {
        [StringLength(256)] public string Email { get; set; }
        [StringLength(256)] public string ApiKey { get; set; }
        [StringLength(256)] public int? JobPeriodTime { get; set; }
        [StringLength(256)] public string JobName { get; set; }
        [StringLength(256)] public string JobConfig { get; set; }
    }
}
