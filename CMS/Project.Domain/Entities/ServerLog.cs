using Project.Domain.Entities.Base;
using Project.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class ServerLog : BaseEntity
    {
        public int ServerId { get; set; }
        public Server Server { get; set; }
        public string UserId { get; set; }
        public string Ip { get; set; }
        public string Isp { get; set; }
        public string City { get; set; }
        public string Org { get; set; }
        public string Country { get; set; }
        public Operator Operator { get; set; }
        public ConnectionStatus ConnectionStatus { get; set; }
    }
}
