using AutoMapper;
using CloudFlare.NET;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.Domain;
using Project.Application.Features.Interfaces;

namespace Project.Application.Features.Services
{
    public class DomainService : IDomainService
    {
        private readonly IDomainRepository _domainRepository;
        private readonly IMapper _mapper;

        public DomainService(IMapper mapper, IDomainRepository domainRepository)
        {
            _mapper = mapper;
            _domainRepository = domainRepository;
        }


        public async Task<List<DomainDTO>> GetAll()
        {
            var list = await _domainRepository.GetAll();
            var model = _mapper.Map<IEnumerable<Domain.Entities.Domain>, List<DomainDTO>>(list.Where(x => !x.IsDeleted));
            return model;
        }

        public async Task<List<DomainDTO>> GetByFilter(int filter)
        {
            var query = _domainRepository.GetAllQueryable();
            query = filter == 1 ? query.Where(x => x.IsActive) : query.Where(x => x.IsActive == false);
            var data = await query.ToListAsync();
            var list = _mapper.Map<IEnumerable<Domain.Entities.Domain>, List<DomainDTO>>(data);
            return list;
        }

        public async Task Create(CreateDomainDTO input)
        {
            var model = _mapper.Map<Domain.Entities.Domain>(input);
            await _domainRepository.Add(model);
        }

        public async Task Delete(int id)
        {
            await _domainRepository.Delete(id);
        }
        public Task Remove()
        {
            var query = _domainRepository.GetAllQueryable();
            query = query.Where(x => !x.IsActive);
            var list = query.ToList();
            foreach (var domain in list)
            {
                _domainRepository.RemoveWithoutSaveChange(domain);
            }

            _domainRepository.SaveChangesTask();
            return Task.CompletedTask;
        }

        public async Task<List<Domain.Entities.Domain>> ListInactiveDomain()
        {
            var listInactive = await GetByFilter(0);
            var list = _mapper.Map<IEnumerable<DomainDTO>, List<Domain.Entities.Domain>>(listInactive);
            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <returns>item 1 : server name  ,
        /// item 2 : host name
        /// </returns>
        private static (string, string) GetServerAndHostStrings(string config, string subServerName, string subHostName)
        {
            var jsonObject = JObject.Parse(config);

            var tlsServerName = jsonObject["outbounds"]?[0]?["streamSettings"]?["tlsSettings"]?["serverName"]?.ToString();
            var serverNameString = tlsServerName?.Split(".");

            var wsHost = jsonObject["outbounds"]?[0]?["streamSettings"]?["wsSettings"]?["headers"]?["Host"]?.ToString();
            var hostString = wsHost?.Split(".");

            var serverName = $"{subServerName}.{serverNameString?[1]}.{serverNameString?[2]}";
            var host = $"{subHostName}.{hostString?[1]}.{hostString?[2]}";
            return (serverNameString?.ToString(), hostString?.ToString());
        }
        public static JObject SetServerAndHostAndAddressStrings(string config, string newServerName, string newHost)
        {
            var jsonObject = JObject.Parse(config);
            jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"] = newServerName;
            jsonObject["outbounds"]![0]!["streamSettings"]!["wsSettings"]!["headers"]!["Host"] = newHost;
            return jsonObject;
        }
        public JObject SetServerAddressStrings(string config, string newAddress)
        {
            var jsonObject = JObject.Parse(config);
            jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["address"] = newAddress;
            return jsonObject;
        }
        public Task UpdateJsonValues(string config, string newServerName, string newHost, string newAddress)
        {
            //var serverAndHost = GetServerAndHostStrings(config);

            SetServerAndHostAndAddressStrings(config, newServerName, newHost);
            SetServerAddressStrings(config, newAddress);
            return Task.CompletedTask;
        }

        private static bool IsNotNullString(string @string)
        {
            return string.IsNullOrWhiteSpace(@string);
        }


        public async Task<string> UpdateCloudflareDnsRecord(string currentDomainValue, string newHost, string cfEmail, string cfApiKey)
        {
            var cfZoneId = string.Empty;
            var cfDomain = $"{currentDomainValue?[1]}.{currentDomainValue?[2]}";
            var auth = new CloudFlareAuth(cfEmail, cfApiKey);
            var cfClient = new CloudFlareClient(auth);
            var zones = await cfClient.GetAllZonesAsync();
            foreach (var zone in zones)
            {
                if (zone.Name == cfDomain)
                    cfZoneId = new IdentifierTag(zone.Id);
            }

            var dnsRecords = await cfClient.GetDnsRecordsAsync(cfZoneId);
            var cnameRecord = dnsRecords.Result.FirstOrDefault(record => record.Type == DnsRecordType.CNAME);
            var cloudflare = new CloudflareApiClient();
            if (cnameRecord != null)
            {
                await cloudflare.UpdateDnsRecordAsync(cfZoneId, cnameRecord.Id, newHost, currentDomainValue, cfApiKey, cfEmail);
            }
            else
            {
                await cloudflare.CreateDnsRecordAsync(cfZoneId, newHost, currentDomainValue, cfApiKey, cfEmail);
            }

            return cfZoneId;
        }

    }
}
