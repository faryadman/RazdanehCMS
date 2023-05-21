using Project.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class CronJobInfo : BaseEntity
    {
        public int Timer { get; set; }
        public int ExecutedCount { get; set; }
        public DateTime LastExecutionDate { get; set; }
    }
}
