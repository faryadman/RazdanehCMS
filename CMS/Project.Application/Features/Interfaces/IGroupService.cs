using Project.Application.DTOs.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Features.Interfaces
{
    public interface IGroupService
    {
        Task<List<GroupDTO>> GetAll();
        Task<List<GroupDTO>> GetFiltered(bool isAd);
        Task Create(CreateGroupDTO input);
        Task Edit(EditGroupDTO input);
        Task Delete(int id);
    }
}
