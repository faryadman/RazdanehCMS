using AutoMapper;
using CloudFlare.NET;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.Domain;
using Project.Application.DTOs.Server;
using Project.Application.Extensions;
using Project.Application.Features.Interfaces;

namespace Project.Application.Features.Services
{
    public class DomainService : IDomainService
    {
        private readonly IDomainRepository _domainRepository;
        private readonly IMapper _mapper;
        private readonly IServerService _serverService;

        public DomainService(IMapper mapper, IDomainRepository domainRepository, IServerService serverService)
        {
            _mapper = mapper;
            _domainRepository = domainRepository;
            _serverService = serverService;
        }


        public async Task<List<DomainDTO>> GetAll()
        {
            var list = await _domainRepository.GetAll();
            var model = _mapper.Map<IEnumerable<Domain.Entities.Domain>, List<DomainDTO>>(list.Where(x => !x.IsDeleted).ToList());
            return model;
        }

        public async Task<List<DomainDTO>> GetByFilter(int filter)
        {
            var query = _domainRepository.GetAllQueryable();
            query = filter == 1 ? query.Where(x => x.IsActive) : query.Where(x => !x.IsActive);
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

        public async Task<List<DomainDTO>> ListInactiveDomain()
        {
            return await GetByFilter(0);
        }
        private static void UpdateJsonValues(JObject config, string newServerName, string newHost)
        {
            const string serverNamePath = "outbounds[0].streamSettings.tlsSettings.serverName";
            const string hostPath = "outbounds[0].streamSettings.wsSettings.headers.Host";

            config.SelectToken(serverNamePath).Replace($"{newServerName}");
            config.SelectToken(hostPath).Replace($"{newHost}");
        }
        public JObject SetServerAddressStrings(string config, string newAddress)
        {
            var jsonObject = JObject.Parse(config);
            jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["address"] = newAddress;
            return jsonObject;
        }
        public async Task<string> ChangeDomain(string serverId, string email, string apiKey)
        {
            try
            {
                var domains = await GetAll();
                if (domains.Count == 0)
                {
                    return "Domain doesn't exist!";
                }

                var server = await _serverService.Detail(serverId);
                var config = server.Config;
                var firstDomain = domains.FirstOrDefault(x => x.IsActive);
                var newDomain = firstDomain.DomainName;

                var cnameValue = await ChangeCnameDomain(email, apiKey, server.CurrentDomainValue, newDomain);

                UpdateServerConfig(server, config, cnameValue, newDomain, newDomain);

                await _serverService.UpdateServer(server);
                await Delete(firstDomain.Id);
                return "Done successfully";
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                return ex.Message;
            }
        }
        public async Task<string> ChangeSubDomain(string serverId, string email, string apiKey)
        {
            try
            {
                var server = await _serverService.Detail(serverId);
                var config = server.Config;
                var jsonObject = JObject.Parse(config);
                var serverNameString = jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"]?.ToString().Split(".");
                var hostString = jsonObject["outbounds"]![0]!["streamSettings"]!["wsSettings"]!["headers"]!["Host"]?.ToString().Split(".");
                var serverName = $"{serverNameString?[1]}.{serverNameString?[2]}";
                var hostName = $"{hostString?[1]}.{hostString?[2]}";

                var cnameValue = await ChangeCnameDomain(email, apiKey, serverName, serverName, true);
                if (cnameValue != "fail")
                {
                    UpdateServerConfig(server, config, cnameValue, serverName, hostName);
                }

                await _serverService.UpdateServer(server);

                return "Done successfully";
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                return ex.Message;
            }
        }
        public async Task<string> DeleteCnameDnsAsync(string serverId, string expireMinuteOn, string email, string apiKey)
        {
            try
            {
                var cfEmail = email;
                var cfApiKey = apiKey;
                var server = await _serverService.Detail(serverId);
                var config = server.Config;
                var jsonObject = JObject.Parse(config);
                var serverNameString = jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"]?.ToString().Split(".");
                var serverName = $"{serverNameString?[1]}.{serverNameString?[2]}";
                var subServerName = $"{serverNameString?[1]}.{serverNameString?[2]}";

                var cfClient = InitializeCloudflareClient(cfEmail, cfApiKey);
                var cfZoneId = await GetCloudflareZoneId(cfClient, subServerName);
                if (await DeleteDnsRecord(cfZoneId, expireMinuteOn, cfEmail, cfApiKey))
                {
                    await ChangeCnameDomain(cfEmail, cfApiKey, server.CurrentDomainValue, serverName, true);
                }
                return "Done successfully";
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                return ex.Message;
            }
        }

        private static async Task<string> ChangeCnameDomain(string email, string apiKey, string currentDomain, string newDomain, bool isActiveSubDomain = false)
        {
            try
            {
                var cfEmail = email;
                var cfApiKey = apiKey;
                var newCnameValue = GenerateWordExtention.GenerateWords(5)[0];
                var cnameValue = $"{newCnameValue}";

                var cfClient = InitializeCloudflareClient(cfEmail, cfApiKey);
                var cfZoneId = await GetCloudflareZoneId(cfClient, newDomain);

                if (cfZoneId == null)
                {
                    return "Zone not found!";
                }

                if (isActiveSubDomain)
                {
                    await CreateDnsRecord(cfZoneId, newCnameValue, currentDomain, cfEmail, cfApiKey);
                }
                else
                {
                    await UpdateDnsRecord(cfClient, cfZoneId, newCnameValue, currentDomain, cfEmail, cfApiKey);

                }

                return cnameValue;
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                return "fail";
            }
        }



        public void UpdateServerConfig(ServerDTO server, string config, string newCnameValue, string newServerName, string newHostName)
        {
            var cnameValue1 = $"{newCnameValue}.{newServerName}";
            var cnameValue2 = $"{newCnameValue}.{newHostName}";

            var configJson = JObject.Parse(config);
            UpdateJsonValues(configJson, cnameValue1, cnameValue2);

            server.Config = configJson.ToString();
            server.IsNewDomain = true;
            server.DomainDateTime = DateTime.UtcNow;
        }


        private static CloudFlareClient InitializeCloudflareClient(string email, string apiKey)
        {
            var auth = new CloudFlareAuth(email, apiKey);
            return new CloudFlareClient(auth);
        }

        private static async Task<string> GetCloudflareZoneId(IZoneClient cfClient, string cfDomain)
        {

            var zones = await cfClient.GetAllZonesAsync();
            var zone = zones.FirstOrDefault(z => z.Name == cfDomain);
            return zone?.Id;
        }

        private static async Task UpdateDnsRecord(IDnsRecordClient cfClient, string cfZoneId, string newCnameValue, string cnameContent, string cfEmail, string cfApiKey)
        {
            var dnsRecords = await cfClient.GetDnsRecordsAsync(cfZoneId);
            var cnameRecord = dnsRecords.Result.FirstOrDefault(record => record.Type == DnsRecordType.CNAME);

            var cloudflare = new CloudflareApiClient();

            if (cnameRecord != null)
            {
                await cloudflare.UpdateDnsRecordAsync(cfZoneId, cnameRecord.Id, newCnameValue, cnameContent, cfApiKey, cfEmail);
            }
            else
            {
                await cloudflare.CreateDnsRecordAsync(cfZoneId, newCnameValue, cnameContent, cfApiKey, cfEmail);
            }
        }
        private static async Task CreateDnsRecord(string cfZoneId, string newCnameValue, string cnameContent, string cfEmail, string cfApiKey)
        {
            var cloudflare = new CloudflareApiClient();
            await cloudflare.CreateDnsRecordAsync(cfZoneId, newCnameValue, cnameContent, cfApiKey, cfEmail);

        }
        private static async Task<bool> DeleteDnsRecord(string cfZoneId, string expireMinuteOn, string cfEmail, string cfApiKey)
        {
            var cloudflare = new CloudflareApiClient();
            return await cloudflare.DeleteCnameRecords(cfZoneId, expireMinuteOn, cfApiKey, cfEmail);
        }
    }
}
