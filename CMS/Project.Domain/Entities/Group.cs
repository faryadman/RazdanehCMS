using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Entities
{
    [Index(nameof(Group.Id), IsUnique = true)]
    public class Group : BaseEntity
    {
        [StringLength(128)]
        public string Title { get; set; }
        public bool IsAd { get; set; }
        public virtual ICollection<Server> servers { get; set; }
    }
}
