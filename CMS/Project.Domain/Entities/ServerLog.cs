using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities.Base;
using Project.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Domain.Entities
{
    [Index(nameof(ServerLog.Id), IsUnique = true)]
    public class ServerLog : BaseEntity
    {
        public int ServerId { get; set; }
        [ForeignKey("ServerId")]
        public virtual Server Server { get; set; }
        [StringLength(128)]
        public string UserId { get; set; }
        [StringLength(128)]
        public string Ip { get; set; }
        [StringLength(128)]
        public string Isp { get; set; }
        [StringLength(128)]
        public string City { get; set; }
        [StringLength(128)]
        public string Org { get; set; }
        [StringLength(128)]
        public string Country { get; set; }
        public Operator Operator { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
    }
}
