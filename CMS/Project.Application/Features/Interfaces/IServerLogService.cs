using Project.Application.DTOs.ApiLog;
using Project.Application.DTOs.ServerLog;
using Project.Domain.Entities;

namespace Project.Application.Features.Interfaces
{
    public interface IServerLogService
    {
        Task Create(AddServerLogDTO input);
        Task<List<ServerLogDTO>> ListByServer(int serverId);
        Task<List<ServerLog>> List();
        Task<ServerLogStatisticsDTO> GetAllLogsStatistics();
        Task DeleteServerLogs(int count = 100000);
        Task Delete(ServerLog log);
        Task Delete(int id);
        void RestServerLogs();
        Task<ApiLogDTO> GetLastLog();
    }
}
