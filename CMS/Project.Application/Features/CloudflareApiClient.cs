using CloudFlare.Client;
using CloudFlare.Client.Api.Zones.DnsRecord;
using Microsoft.Extensions.Options;
using Project.Application.Exceptions;
using Project.Domain.Entities;

namespace Project.Application.Features
{
    public class CloudflareApiClient
    {
        private readonly string _apiKey;
        private readonly string _email;

        public CloudflareApiClient(IOptions<CloudflareSettings> options)
        {
            var settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.Email))
                throw new InvalidOperationException("CloudflareData:ApiKey or CloudflareData:Email is missing. Configure them in secrets/environment.");

            _apiKey = settings.ApiKey;
            _email = settings.Email;
        }

        private CloudFlareClient CreateClient()
        {
            return new CloudFlareClient(_email, _apiKey);
        }
        public async Task<bool> IsExistCnameRecord(string zoneId, CloudFlare.Client.Enumerators.DnsRecordType type , CancellationToken ct)
        {
            try
            {
                using var client = CreateClient();
                var zones = await client.Zones.GetAsync(cancellationToken: ct);
                var dnsRecords = await client.Zones.DnsRecords.GetAsync(zoneId, cancellationToken: ct);
                foreach (var dnsRecord in dnsRecords.Result)
                {
                    if (dnsRecord.Type == type)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {

                throw new NotFoundException("Record not found.");
            }
        }
        public async Task<CnameRecord> GetCnameRecord(string zoneId, CloudFlare.Client.Enumerators.DnsRecordType type, CancellationToken ct)
        {
            try
            {
                using var client = CreateClient();
                var zones = await client.Zones.GetAsync(cancellationToken: ct);
                var dnsRecords = await client.Zones.DnsRecords.GetAsync(zoneId, cancellationToken: ct);
                foreach (var dnsRecord in dnsRecords.Result)
                {
                    if (dnsRecord.Type == type)
                    {
                        return new CnameRecord
                        {
                            id = dnsRecord.Id,
                            name = dnsRecord.Name,
                            content = dnsRecord.Content
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {

                throw new NotFoundException("Record not found.");
            }
        }
        public async Task UpdateDnsRecordAsync(
        string zoneId,
        string recordId,
        string newCname,
        string cnameContent,
        CancellationToken ct)
        {
            {
                using var client = CreateClient();

                var zones = await client.Zones.GetAsync(cancellationToken: ct);
                var dnsRecords = await client.Zones.DnsRecords.GetAsync(zoneId, cancellationToken: ct);

                foreach (var dnsRecord in dnsRecords.Result)
                {
                    if (dnsRecord.Id == recordId)
                    {
                        var modifed = new ModifiedDnsRecord
                        {
                            Type = CloudFlare.Client.Enumerators.DnsRecordType.Cname,
                            Name = newCname,
                            Content = cnameContent,
                            Ttl = 1,
                            Proxied = false,
                            Comment = DateTime.Now.ToString()

                        };
                        var updateResponse = await client.Zones.DnsRecords.UpdateAsync(zoneId, recordId, modifed, cancellationToken: ct);
                        if (updateResponse.Success)
                        {
                            Console.WriteLine("DNS record updated successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Failed to update DNS record. Errors: " + string.Join(", ", updateResponse.Errors.Select(e => e.Message)));
                        }
                        return;
                    }

                }
                    throw new NotFoundException("Record not found.");
            }
        }
        

        public async Task CreateDnsRecordAsync(string zoneId, string newCname, string cnameContent,CancellationToken ct)
        {
            using var client = CreateClient();

            var zones = await client.Zones.GetAsync(cancellationToken: ct);
            var dnsRecords = await client.Zones.DnsRecords.GetAsync(zoneId, cancellationToken: ct);
            var newDnsRecord                         = new NewDnsRecord
            {
                Type = CloudFlare.Client.Enumerators.DnsRecordType.Cname,
                Name = newCname,
                Content = cnameContent,
                Ttl = 1,
                Proxied = false,
                Comment = DateTime.Now.ToString()
            };
            var updateResponse = await client.Zones.DnsRecords.AddAsync(zoneId,   newDnsRecord  , cancellationToken: ct);
            if (updateResponse.Success)
            {
                Console.WriteLine("DNS record updated successfully.");
                return;
            }
            else
            {
                Console.WriteLine("Failed to update DNS record. Errors: " + string.Join(", ", updateResponse.Errors.Select(e => e.Message)));
            }
            throw new NotFoundException("Failed to create DNS record.");
        }
        public async Task<IList<CnameRecord>> GetAllZonesAsync(string domainName,CancellationToken ct)
        {
            try
            {
                using var client = CreateClient();

                var zones = await client.Zones.GetAsync(new CloudFlare.Client.Api.Zones.ZoneFilter { Name = domainName }, cancellationToken: ct);
                return zones.Result.Select(zone => new CnameRecord
                {
                    id = zone.Id,
                    name = zone.Name,
                    content = zone.Status.ToString()
                }).ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw new NotFoundException("Failed to get DNS records.");
            }
        }
        public async Task<IList<CnameRecord>> GetAllZonesAsync(CancellationToken ct)
        {
            try
            {
                using var client = CreateClient();

                var zones = await client.Zones.GetAsync(cancellationToken: ct);
                return zones.Result.Select(zone => new CnameRecord
                {
                    id = zone.Id,
                    name = zone.Name,
                    content = zone.Status.ToString()
                }).ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw new NotFoundException("Failed to get DNS records.");
            }
        }
        public async Task<IList<CnameRecord>> GetAllRecords(string zoneId, CancellationToken ct)
        {
            try
            {
                using var client = CreateClient();

                var zones = await client.Zones.GetAsync(cancellationToken: ct);
                var dnsRecords = await client.Zones.DnsRecords.GetAsync(zoneId, cancellationToken: ct);
                return dnsRecords.Result.Select(record => new CnameRecord
                {
                    id = record.Id,
                    name = record.Name,
                    content = record.Content
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw new NotFoundException("Failed to get DNS records.");
            }
        }
        public async Task DeleteCnameRecords(string zoneId, string recordToDeleteId, CancellationToken ct)
        {
            try
            {
          
                using var client = CreateClient();

                var zones = await client.Zones.GetAsync(cancellationToken: ct);
                var dnsRecords = await client.Zones.DnsRecords.DeleteAsync(zoneId, recordToDeleteId, cancellationToken: ct);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw new NotFoundException("Failed to delete DNS record.");    
            }
        }

  

        public class CnameRecord
        {
            public string id { get; set; }

            public string name { get; set; }

            public string content { get; set; }

        }

        public class ResultInfo
        {
            public int Count { get; set; }
        }
    }
}