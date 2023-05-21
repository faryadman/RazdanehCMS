using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.ServerLog
{
    public class ServerLogStatistics
    {
        public int Count { get; set; }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
    }
}
