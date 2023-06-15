using Project.Application.DTOs.ApiLog;

namespace Project.Application.Features.Interfaces
{
    public interface IApiLogService
    {
        Task Create(ApiLogDTO input);
        Task<ApiLogDTO> GetLastLog(int appSettingId);
        Task DeleteApiLog(int count = 100000);
    }
}
