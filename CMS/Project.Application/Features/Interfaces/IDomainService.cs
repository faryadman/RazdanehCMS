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
        Task<string> ChangeDomain(string zoneId);
        Task<string> ChangeSubDomain(string serverId);
        Task<string> GenerateDnsAsync(string serverId, string expireMinuteOn);
        Task<string> DeleteDnsAsync(string serverId = null, string expireMinuteOn = null);
        Task<string> CreateDnsAsync(string serverId);
        Task ChangeSubDomain();
        Task ChangeDomain();
        Task InitZoneId();
    }
}