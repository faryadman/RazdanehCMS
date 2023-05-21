using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.ServerLog
{
    public class ServerLogStatisticsDTO
    {
        public ServerLogStatistics AllLogsStatistics { get; set; }
        public ServerLogStatistics IrancellLogsStatistics { get; set; }
        public ServerLogStatistics HamraheAvvalLogsStatistics { get; set; }
        public ServerLogStatistics UnknownLogsStatistics { get; set; }
    }
}
