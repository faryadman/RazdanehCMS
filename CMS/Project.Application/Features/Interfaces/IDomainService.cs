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
        string GetServerAddressStrings(string config);
        Task<string> ChangeDomain(string serverId, string email, string apiKey);
        Task<string> ChangeSubDomain(string serverId, string email, string apiKey);
        Task<string> GenerateDnsAsync(string serverId, string expireMinuteOn, string email, string apiKey);
        Task<string> DeleteDnsAsync(string serverId, string email, string apiKey);
        Task<string> CreateDnsAsync(string serverId, string email, string apiKey);
    }
}