using Project.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Entities
{
    public class SaveIP : BaseEntity
    {
        [StringLength(128)]
        public string Ip { get; set; }
        [StringLength(128)]
        public string Tcp { get; set; }
        public string UserAgent { get; set; }
    }
}
