using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Domain.Entities
{
    [Index(nameof(Server.Id), IsUnique = true)]
    public class Server : BaseEntity
    {
        [StringLength(128)]
        public string ServerName { get; set; }
        [StringLength(128)]
        public string Location { get; set; }
        [StringLength(128)]
        public string Ip { get; set; }
        public string Config { get; set; }
        [StringLength(128)]
        public string ConfigKey { get; set; }
        [StringLength(128)]
        public string ConfigValue { get; set; }
        //public string CurrentDomainValue { get; set; }
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }
        public int GroupId { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsAd { get; set; }
        [StringLength(256)]
        public string CurrentDomainValue { get; set; }
        public bool IsNewDomain { get; set; }
        public bool IsForIrancell { get; set; }
        public bool IsForHamraheAvval { get; set; }
        public virtual ICollection<ServerLog> Logs { get; set; }
        public DateTime DomainDateTime { get; set; } = DateTime.UtcNow;
    }
}
