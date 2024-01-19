using AutoMapper;
using CloudFlare.NET;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        // دیکشنری لوکال برای ذخیره زون‌ها با استفاده از نام دامنه به عنوان کلید
        private static readonly Dictionary<string, string> zoneDictionary = new Dictionary<string, string>();

        public DomainService(IMapper mapper, IDomainRepository domainRepository, IServerService serverService, IConfiguration configuration)
        {
            _mapper = mapper;
            _domainRepository = domainRepository;
            _serverService = serverService;
            _configuration = configuration;
        }

        public async Task InitZoneId()
        {
            var domains = await GetAll();
            foreach (var domain in from domain in domains where domain != null where domain.IsActive != false where string.IsNullOrEmpty(domain.ZoneId) select domain)
            {
                domain.ZoneId = await GetCloudflareZoneId(domain.DomainName);
                await Update(domain);
            }
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

        public async Task<List<DomainDTO>> ListInactiveDomain()
        {
            return await GetByFilter(0);
        }

        public JObject SetServerAddressStrings(string config, string newAddress)
        {
            var jsonObject = JObject.Parse(config);
            jsonObject["outbounds"]![0]!["settings"]!["vnext"]![0]!["address"] = newAddress;
            return jsonObject;
        }

        public string GetServerAddressStrings(string config)
        {
            var jsonObject = JObject.Parse(config);
            return jsonObject["outbounds"]![0]!["settings"]!["vnext"]![0]!["address"]!.ToString();
        }

        public async Task<string> ChangeDomain(string serverId)
        {
            try
            {
                var newDomainDto = await GetNewDomain();
                if (newDomainDto == null) return "No Domain Exist!";
                var server = await _serverService.Detail(serverId);
                var zoneId = newDomainDto.ZoneId;
                var config = server.Config;
                var newDomain = newDomainDto.DomainName;
                var cnameValue = await ChangeCnameDomain(server.CurrentDomainValue, zoneId);

                UpdateServerConfig(server, config, cnameValue, newDomain, newDomain);

                await _serverService.UpdateServer(server);
                await Delete(newDomainDto.Id);
                return "Done successfully";
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                return ex.Message;
            }
        }

        public async Task<string> ChangeSubDomain(string serverId)
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
                var domainDto = await GetDomain(serverName);
                var cnameValue = await ChangeCnameDomain(server.CurrentDomainValue, domainDto.ZoneId, true);
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
        public async Task<string> DeleteDnsAsync(string serverId, string expireTime)
        {
            try
            {
                if (serverId != null)
                {
                    var cfZoneId = ServerDto(serverId, out var server, out var serverName);
                    await DeleteDnsRecord(cfZoneId);
                    await ChangeSubDomain(server.Id.ToString());
                }
                else
                {
                    var serverIds = await _serverService.GetActiveIds();
                    foreach (var id in serverIds)
                    {
                        await GenerateDnsAsync(id.ToString(), expireTime);
                    }
                }

                return "Done successfully";
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                return ex.Message;
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

        public async Task<string> CreateDnsAsync(string serverId)
        {
            try
            {
                var zoneId = ServerDto(serverId, out var server, out var serverName);
                var cnameValue = await ChangeCnameDomain(server.CurrentDomainValue, zoneId, true);
                UpdateServerConfig(server, server.Config, cnameValue, serverName, serverName);
                await _serverService.UpdateServer(server);
                return "Done successfully";
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                return ex.Message;
            }
        }
        public async Task ChangeSubDomain()
        {
            var serverIds = await _serverService.GetActiveIds();
            foreach (var id in serverIds)
            {
                await ChangeSubDomain(id.ToString());
            }
        }

        public async Task<string> GenerateDnsAsync(string serverId, string expireMinuteOn)
        {
            try
            {
                await DeleteAndCheckDnsAsync(serverId, expireMinuteOn, authCfValueTuple().email, authCfValueTuple().apiKey);
                await CreateDnsAsync(serverId);
                return "Done successfully";
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                return ex.Message;
            }
        }

        public async Task<bool> DeleteDnsRecord(string cfZoneId)
        {
            var cloudflare = new CloudflareApiClient();
            var recordsToDelete = await cloudflare.GetAllRecords(cfZoneId, authCfValueTuple().apiKey, authCfValueTuple().email);
            foreach (var recordToDelete in recordsToDelete)
            {
                await cloudflare.DeleteCnameRecords(cfZoneId, recordToDelete.id, authCfValueTuple().apiKey, authCfValueTuple().email);
            }

            return true;
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
        private async Task<DomainDTO> GetDomain(string name)
        {
            var domains = await GetAll();
            if (domains.Count == 0)
            {
                return null;
            }
            return domains.FirstOrDefault(x => x.DomainName == name);
        }
        private async Task<DomainDTO> GetNewDomain()
        {
            var domains = await GetAll();
            if (domains.Count == 0)
            {
                return null;
            }
            return domains.FirstOrDefault(x => x.IsActive);
        }
        private async Task<string> ChangeCnameDomain(string currentDomain, string zoneId, bool isActiveSubDomain = false)
        {
            try
            {

                var newCnameValue = GenerateWordExtention.GenerateWords(5)[0];
                var cnameValue = $"{newCnameValue}";

                if (isActiveSubDomain)
                {
                    await CreateDnsRecord(zoneId, newCnameValue, currentDomain);
                }
                else
                {
                    await UpdateDnsRecord(zoneId, newCnameValue, currentDomain);
                }

                return cnameValue;
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                return "fail";
            }
        }
        private async Task<string> DeleteAndCheckDnsAsync(string serverId, string minuteTimeOn, string email, string apiKey)
        {
            try
            {
                var cfZoneId = ServerDto(serverId, out var server, out var serverName);
                var cloudflare = new CloudflareApiClient();
                var recordsToDelete = await cloudflare.GetAllRecords(cfZoneId, apiKey, email);
                foreach (var record in recordsToDelete)
                {
                    if (!string.IsNullOrEmpty(record.comment))
                    {
                        var modifiedDateTime = DateTime.Parse(record.comment);
                        var expireTimeInMinutes = double.Parse(minuteTimeOn);
                        var expireTimeSpan = TimeSpan.FromMinutes(expireTimeInMinutes);

                        var currentTime = DateTime.Now;
                        var timeDifference = currentTime - modifiedDateTime;
                        if (timeDifference >= expireTimeSpan)
                        {
                            await cloudflare.DeleteCnameRecords(cfZoneId, record.id, apiKey, email);
                        }
                    }
                }
                return "Done successfully";
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                return ex.Message;
            }
        }

        private string ServerDto(string serverId, out ServerDTO server, out string serverName)
        {
            server = _serverService.Detail(serverId).GetAwaiter().GetResult();
            var config = server.Config;
            var jsonObject = JObject.Parse(config);
            var serverNameString = jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"]?.ToString()
                .Split(".");
            serverName = $"{serverNameString?[1]}.{serverNameString?[2]}";
            return GetDomain(serverName).Result.ZoneId;
        }

        private static void UpdateJsonValues(JObject config, string newServerName, string newHost)
        {
            const string serverNamePath = "outbounds[0].streamSettings.tlsSettings.serverName";
            const string hostPath = "outbounds[0].streamSettings.wsSettings.headers.Host";

            config.SelectToken(serverNamePath)?.Replace($"{newServerName}");
            config.SelectToken(hostPath)?.Replace($"{newHost}");
        }

        private static CloudFlareClient InitializeCloudflareClient(string email, string apiKey)
        {
            var auth = new CloudFlareAuth(email, apiKey);
            return new CloudFlareClient(auth);
        }

        private (string apiKey, string email) authCfValueTuple() => (apiKey: _configuration["CloudflareData:ApiKey"], email: _configuration["CloudflareData:Email"]);

        private CloudFlareClient GetCloudFlareClient()
        {
            var apiKey = authCfValueTuple().apiKey;
            var email = authCfValueTuple().email;
            return InitializeCloudflareClient(email, apiKey);
        }

        private async Task<string> GetCloudflareZoneId(string cfDomain)
        {
            // اگر لیست زون‌ها خالی باشد، ابتدا آن را دریافت کنید
            if (zoneDictionary.Count == 0)
            {
                var cfClient1 = GetCloudFlareClient();
                var zones = await cfClient1.GetAllZonesAsync();
                foreach (var zone1 in zones)
                {
                    zoneDictionary[zone1.Name] = zone1.Id;
                }
            }
            // اگر زون با این دامنه در دیکشنری وجود داشته باشد، آن را بازگردانی کنید
            if (zoneDictionary.TryGetValue(cfDomain, out var zoneId))
            {
                return zoneId;
            }

            var cfClient2 = GetCloudFlareClient();
            // در غیر این صورت، زون مربوط به دامنه را جستجو و به دیکشنری اضافه کنید
            var zone = cfClient2.GetAllZonesAsync().Result.FirstOrDefault(z => z.Name == cfDomain);
            if (zone != null)
            {
                zoneDictionary[zone.Name] = zone.Id;
                return zone.Id;
            }
            // اگر زون پیدا نشد، مقدار خالی یا یک مقدار پیش‌فرض (بسته به نیاز شما) بازگردانی شود
            return string.Empty;
        }

        private async Task UpdateDnsRecord(string cfZoneId, string newCnameValue, string cnameContent)
        {
            var cfClient = GetCloudFlareClient();
            var dnsRecords = await cfClient.GetDnsRecordsAsync(cfZoneId);
            var cnameRecord = dnsRecords.Result.FirstOrDefault(record => record.Type == DnsRecordType.CNAME);

            var cloudflare = new CloudflareApiClient();

            if (cnameRecord != null)
            {
                await cloudflare.UpdateDnsRecordAsync(cfZoneId, cnameRecord.Id, newCnameValue, cnameContent, authCfValueTuple().apiKey, authCfValueTuple().email);
            }
            else
            {
                await cloudflare.CreateDnsRecordAsync(cfZoneId, newCnameValue, cnameContent, authCfValueTuple().apiKey, authCfValueTuple().email);
            }
        }

        private async Task CreateDnsRecord(string cfZoneId, string newCnameValue, string cnameContent)
        {
            var cloudflare = new CloudflareApiClient();
            await cloudflare.CreateDnsRecordAsync(cfZoneId, newCnameValue, cnameContent, authCfValueTuple().apiKey, authCfValueTuple().email);
        }

        private async Task Update(DomainDTO domain)
        {
            var model = await _domainRepository.SingleOrDefaultAsync(x => x.Id == domain.Id);
            model.DomainName = domain.DomainName;
            model.ZoneId = domain.ZoneId;
            await _domainRepository.Update(model);
        }
    }
}
