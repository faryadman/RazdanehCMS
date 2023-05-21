using Project.Application.DTOs.ServerLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Features.Interfaces
{
    public interface IServerLogServie
    {
        Task Create(AddServerLogDTO input);
        Task<List<ServerLogDTO>> ListByServer(int serverId);
        Task<List<ServerLogDTO>> List();
        Task<ServerLogStatisticsDTO> GetAllLogsStatistics();
    }
}
