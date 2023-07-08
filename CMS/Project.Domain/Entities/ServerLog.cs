using Project.Domain.Entities.Base;
using Project.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Project.Domain.Entities
{
    public class ServerLog : BaseEntity
    {
        public int ServerId { get; set; }
        public Server Server { get; set; }
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
