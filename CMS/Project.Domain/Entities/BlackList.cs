using Project.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Domain.Entities
{
    public class BlackList : BaseEntity
    {
        public int ServerId { get; set; }
        [ForeignKey("ServerId")]
        public virtual Server Server { get; set; }
        public string Ip { get; set; }
    }
}
