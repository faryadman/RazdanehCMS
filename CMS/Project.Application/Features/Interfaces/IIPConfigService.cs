using Project.Application.DTOs.IpConfig;
using Project.Domain.Entities;

namespace Project.Application.Features.Interfaces
{
    public interface IIPConfigService
    {
        IPConfigEntity Get(int? id);
        Task<List<IPConfigEntity>> GetAll();
        Task Create(IPConfigDTO input);
        Task Delete(int id);
        Task UpdateIpServers();
    }
}
