using Project.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Entities
{
    public class IPConfigEntity : BaseEntity
    {
        [StringLength(64)] public string IP { get; set; }
        [StringLength(256)] public string FileName { get; set; }
        public bool IsDeleted { get; set; }
    }
}
