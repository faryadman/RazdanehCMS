using Project.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class ApiLog : BaseEntity
    {
        public AppSetting AppSetting { get; set; }
        public int AppSettingId { get; set; }
        public Server Server { get; set; }
        public int ServerId { get; set; }
    }
}
