using Project.Application.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.CronJobInfo
{
    public class CronJobInfoDTO : BaseDTO
    {
        public int Timer { get; set; }
        public int ExecutedCount { get; set; }
        public DateTime LastExecutionDate { get; set; }
        public string LastExecutionDateToString { get; set; }
    }
}
