using Project.Application.DTOs.Server;
using Project.Application.DTOs.ServerLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Features.Interfaces
{
    public interface IServerService
    {
        Task<List<ServerDTO>> GetWithFilter(int? groupId, int? appId, bool isAd,int filter = 0);
        Task<List<int>> GetAllIds();
        Task<ServerDTO> GetByApp(string apiRoute, bool isAd, string isp, string Operator);
        Task Create(CreateServerDTO input);
        Task<ServerDTO> Detail(int id);
        Task Delete(int id);
        Task Duplicate(int id);
        Task DeleteByGroupId(int groupId);
        Task Edit(EditServerDTO input);
        Task ToggleIsAvailableInput(int id);
    }
}
