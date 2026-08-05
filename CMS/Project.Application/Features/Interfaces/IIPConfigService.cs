using Project.Application.DTOs.IpConfig;
using Project.Domain.Entities;

namespace Project.Application.Features.Interfaces
{
    public interface IIPConfigService
    {
        IPConfigEntity Get(int? id);
        Task<List<IPConfigEntity>> GetAll();
        IEnumerable<IPConfigEntity> ListInactive();
        Task Create(IPConfigDTO input);
        Task Delete(int id);
        Task GenerateUpdateAsync(string expireMinuteOn, string email, string apiKey);
    }
}
