using Project.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class BlackList : BaseEntity
    {
        public int ServerId { get; set; }
        public Server Server { get; set; }
        public string Ip { get; set; }
    }
}
