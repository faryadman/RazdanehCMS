using Project.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Entities
{
    public class Group : BaseEntity
    {
        [StringLength(128)]
        public string Title { get; set; }
        public bool IsAd { get; set; }
        public ICollection<Server> servers { get; set; }
    }
}
