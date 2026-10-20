#nullable enable
using CloudFlare.Client.Enumerators;
using static Project.Application.Features.CloudflareApiClient;

namespace Project.Application.Features;

public interface ICloudflareApiClient
{
    Task<CnameRecord?> GetCnameRecord(string zoneId, DnsRecordType type, CancellationToken ct);
    Task UpdateDnsRecordAsync(string zoneId, string recordId, string newCname, string cnameContent, CancellationToken ct);
    Task CreateDnsRecordAsync(string zoneId, string newCname, string cnameContent, CancellationToken ct);
    Task<IList<CnameRecord>> GetAllZonesAsync(string domainName, CancellationToken ct);
    Task<IList<CnameRecord>> GetAllRecords(string zoneId, CancellationToken ct);
    Task DeleteCnameRecords(string zoneId, string recordToDeleteId, CancellationToken ct);
}
