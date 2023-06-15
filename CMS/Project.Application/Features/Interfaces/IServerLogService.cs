using Project.Application.DTOs.ServerLog;

namespace Project.Application.Features.Interfaces
{
    public interface IServerLogService
    {
        Task Create(AddServerLogDTO input);
        Task<List<ServerLogDTO>> ListByServer(int serverId);
        Task<List<ServerLogDTO>> List();
        Task<ServerLogStatisticsDTO> GetAllLogsStatistics();
        Task DeleteServerLogs(int count = 100000);
    }
}
