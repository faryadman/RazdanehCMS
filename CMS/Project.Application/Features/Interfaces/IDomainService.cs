using Newtonsoft.Json.Linq;
using Project.Application.DTOs.Domain;

namespace Project.Application.Features.Interfaces
{
    public interface IDomainService
    {
        Task<List<DomainDTO>> GetAll();
        Task<List<DomainDTO>> GetByFilter(int filter);
        Task Create(CreateDomainDTO input);
        Task Delete(int id);
        Task Remove();
        Task<List<DomainDTO>> ListInactiveDomain();
        JObject SetServerAddressStrings(string config, string newAddress);
        Task<string> ChangeDomain(int serverId, string email, string apiKey);
        Task<string> ChangeSubDomain(int serverId, string email, string apiKey);
        Task<string> DeleteCnameDnsAsync(int serverId, string expireMinuteOn, string email, string apiKey);
    }
}