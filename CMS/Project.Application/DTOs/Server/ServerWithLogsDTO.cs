using Project.Application.DTOs.Base;
using Project.Application.DTOs.Group;
using Project.Application.DTOs.ServerLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.Server
{
    public class ServerWithLogsDTO : BaseDTO
    {
        public string ServerName { get; set; }
        public string Location { get; set; }
        public string Ip { get; set; }
        public string Config { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public string Group { get; set; }
        public List<ServerLogDTO> Logs { get; set; }
    }
}
