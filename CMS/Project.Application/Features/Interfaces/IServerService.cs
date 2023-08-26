using Project.Application.DTOs.Server;
using Project.Application.DTOs.ServerLog;

namespace Project.Application.Features.Interfaces
{
    public interface IServerService
    {
        Task<List<ServerDTO>> GetWithFilter(int? groupId, int? appId, bool isAd, int filter = 0);
        Task<List<int>> GetAllIds();
        Task<List<int>> GetActiveIds();
        Task<ServerDTO> GetByApp(string apiRoute, bool isAd, string isp, string Operator);
        Task<ServerDTO> GetServerStatistics(int serverId);
        Task Create(CreateServerDTO input);
        Task<ServerDTO> Detail(int id);
        Task<ServerDTO> Detail(string id);
        Task Delete(int id);
        Task Duplicate(int id);
        Task DeleteByGroupId(int groupId);
        Task Edit(EditServerDTO input);
        Task ToggleIsAvailableInput(int id);
        Task SuccessServerLog(AddServerLogDTO input);
        Task FailedServerLog(AddServerLogDTO input);
    }
}
