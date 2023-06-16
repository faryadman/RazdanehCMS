using Project.Application.DTOs.Base;
using Project.Application.DTOs.Group;
using Project.Application.DTOs.ServerLog;

namespace Project.Application.DTOs.Server
{
    public class ServerDTO : BaseDTO
    {
        public string ServerName { get; set; }
        public string Location { get; set; }
        public string Ip { get; set; }
        public string Config { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public string CurrentDomainValue { get; set; }
        public bool IsAd { get; set; }
        public bool IsNewDomain { get; set; }
        public bool IsAvailable { get; set; }
        public GroupDTO Group { get; set; }
        public int GroupId { get; set; }
        public bool IsForIrancell { get; set; }
        public bool IsForHamraheAvval { get; set; }
        public DateTime DomainDateTime { get; set; }
        public ServerLogStatistics AllLogsStatistics { get; set; }
        public ServerLogStatistics IrancellLogsStatistics { get; set; }
        public ServerLogStatistics HamraheAvvalLogsStatistics { get; set; }
        public ServerLogStatistics UnknownLogsStatistics { get; set; }
    }
}
