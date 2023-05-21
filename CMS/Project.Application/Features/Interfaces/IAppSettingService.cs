using Project.Application.DTOs.AppSetting;
using Project.Application.DTOs.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Features.Interfaces
{
    public interface IAppSettingService
    {
        Task<List<MinimalAppSettingDTO>> GetAll();
        Task Create(CreateAppSettingDTO input);
        Task<AppSettingDTO> Detail(int id);
        Task<AppSettingDTO> DetailByApiRoute(string apiRoute);
        Task<List<GroupsByAppDTO>> GroupsByApp(int id);
        Task Edit(EditAppSettingDTO input, int id);
        Task UpdateAppGroups(int id,string groupIds);
        Task Delete(int id);
    }
}
