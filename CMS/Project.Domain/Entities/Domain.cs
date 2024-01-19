using Project.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Entities
{
    public class Domain : BaseEntity
    {
        [StringLength(256)] public string DomainName { get; set; }
        [StringLength(256)] public string FileName { get; set; }
        [StringLength(128)] public string ZoneId { get; set; }
        public bool IsDeleted { get; set; }
    }
}