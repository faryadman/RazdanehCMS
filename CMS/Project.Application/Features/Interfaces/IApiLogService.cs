using Project.Application.DTOs.ApiLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Features.Interfaces
{
    public interface IApiLogService
    {
        Task Create(ApiLogDTO input);
        Task<ApiLogDTO> GetLastLog(int appSettingId);
    }
}
