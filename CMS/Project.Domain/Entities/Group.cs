using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Domain.Entities.Base;

namespace Project.Domain.Entities
{
    public class Group : BaseEntity
    {
        public string Title { get; set; }
        public bool IsAd { get; set; }
        public ICollection<Server> servers { get; set; }
    }
}
