using Project.Application.DTOs.BlackList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Features.Interfaces
{
    public interface IBlackListService
    {
        Task Create(int serverId,string ip);
        Task Delete(int id);
        Task<List<BlackListDTO>> List(int? serverId);
    }
}
