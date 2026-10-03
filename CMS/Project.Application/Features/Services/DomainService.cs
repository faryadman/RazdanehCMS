using AutoMapper;
using CloudFlare.Client.Client.Zones;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.Domain;
using Project.Application.DTOs.Server;
using Project.Application.Exceptions;
using Project.Application.Extensions;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;
using Serilog;
using System.Xml.Linq;

namespace Project.Application.Features.Services
{
    public class DomainService : IDomainService
    {
        private readonly IDomainRepository _domainRepository;
        private readonly IMapper _mapper;
        private readonly IServerService _serverService;
        private readonly IConfiguration _configuration;
        private readonly CloudflareApiClient _cloudflareApiClient;
        // دیکشنری لوکال برای ذخیره زون‌ها با استفاده از نام دامنه به عنوان کلید
        private static readonly Dictionary<string, string> zoneDictionary = new Dictionary<string, string>();

        public DomainService(IMapper mapper, IDomainRepository domainRepository, IServerService serverService, IConfiguration configuration, CloudflareApiClient cloudflareApiClient)
        {
            _mapper = mapper;
            _domainRepository = domainRepository;
            _serverService = serverService;
            _configuration = configuration;
            _cloudflareApiClient = cloudflareApiClient;
        }

        public async Task InitZoneId()
        {
            var domains = await GetAll();
            foreach (var domain in from domain in domains where domain != null where domain.IsActive != false where string.IsNullOrEmpty(domain.ZoneId) select domain)
            {
                domain.ZoneId = await GetCloudflareZoneId(domain.DomainName);
                if (string.IsNullOrEmpty(domain.ZoneId))
                {
                    Log.Error($"ZoneId not found for domain: {domain.DomainName}");
                    throw new NotFoundException($"ZoneId not found for domain: {domain.DomainName}");
                }
                await Update(domain);
            }
        }
        public async Task CheckZoneId()
        {
            var queryable = _domainRepository.FindQueryable(a => string.IsNullOrEmpty(a.ZoneId) && a.IsActive == true).Select(
                d => new DomainDTO()
                {
                    IsActive = true,
                    ZoneId = null,
                    DomainName = d.DomainName,
                    FileName = d.FileName,
                    Id = d.Id,
                    IsDeleted = false,
                    UpdatedAt = DateTime.Now
                });
            var domains = await queryable.ToListAsync();
            foreach (var domain in domains)
            {
                domain.ZoneId = await GetCloudflareZoneId(domain.DomainName);
                if (string.IsNullOrEmpty(domain.ZoneId))
                {
                    Log.Error($"ZoneId not found for domain: {domain.DomainName}");
                    throw new NotFoundException($"ZoneId not found for domain: {domain.DomainName}");
                }
                await Update(domain);
            }

        }

        public async Task<List<DomainDTO>> GetAll()
        {
            var list = await _domainRepository.GetAll();
            var model = _mapper.Map<IEnumerable<Domain.Entities.Domain>, List<DomainDTO>>(list.Where(x => x.IsDeleted == false && x.IsActive == true));
            return model;
        }

        public async Task<List<DomainDTO>> GetByFilter(int filter)
        {
            var query = _domainRepository.GetAllQueryable();
            query = filter == 1 ? query.Where(x => x.IsActive == true && x.IsDeleted == false) : query.Where(x => x.IsActive == true && x.IsDeleted == false);
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
                if (newDomainDto == null || string.IsNullOrEmpty(newDomainDto.DomainName) || !newDomainDto.IsActive || newDomainDto.IsDeleted)
                    throw new NotFoundException();
                var server = await _serverService.Detail(serverId);
                var zoneId = newDomainDto.ZoneId;
                var config = server.Config;
                var newDomain = newDomainDto.DomainName;
                var cnameValue = await ChangeCnameDomain(server.CurrentDomainValue, zoneId);

                UpdateServerConfig(server, config, cnameValue, newDomain, newDomain);

                await _serverService.UpdateServer(server);
                await Delete(newDomainDto.Id);
                return "انجام شد.";
            }
            catch (NotFoundException)
            {
                throw new NotFoundException("دامنه غیرفعال یا حذف شده است.");
                throw;
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                throw new NotFoundException("سرور یافت نشد");
            }
        }
        public async Task<string> ChangeTcp(string serverId)
        {
            try
            {

                var server = await _serverService.Detail(serverId);
                if (server == null)
                    throw new NotFoundException("سرور یافت نشد");
                var config = server.Config;
                var domain = await GetNewDomain();
                if (domain == null || domain.DomainName == null || !domain.IsActive || domain.IsDeleted)
                    throw new NotFoundException();
                var zoneId = domain.ZoneId;
                var newDomain = domain.DomainName;
                var cnameValue = await ChangeCnameDomain(server.CurrentDomainValue, zoneId);

                UpdateTcpServerConfig(server, config, newDomain, cnameValue);

                await _serverService.UpdateServer(server);
                await Delete(domain.Id);
                return "انجام شد.";
            }
            catch (NotFoundException)
            {
                throw new NotFoundException("دامنه غیرفعال یا حذف شده است.");
                throw;
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                throw new NotFoundException("دامنه یافت نشد");
            }
        }
        public async Task<string> ChangeSubDomain(string serverId)
        {
            try
            {

                var server = await _serverService.Detail(serverId);
                if (server == null)
                    throw new NotFoundException("سرور یافت نشد");
                var config = server.Config;
                var jsonObject = JObject.Parse(config);
                var serverNameString = jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"]?.ToString().Split(".");
                var hostString = jsonObject["outbounds"]![0]!["streamSettings"]!["wsSettings"]!["headers"]!["Host"]?.ToString().Split(".");
                var serverName = $"{serverNameString?[1]}.{serverNameString?[2]}";
                var hostName = $"{hostString?[1]}.{hostString?[2]}";
                var domainDto = await GetDomain(serverName);
                if (domainDto == null || string.IsNullOrEmpty(domainDto.DomainName) || !domainDto.IsActive || domainDto.IsDeleted)
                    throw new NotFoundException();
                var cnameValue = await ChangeCnameDomain(server.CurrentDomainValue, domainDto.ZoneId, true);
                if (cnameValue != "fail")
                {
                    UpdateServerConfig(server, config, cnameValue, serverName, hostName);
                }

                await _serverService.UpdateServer(server);
                await Delete(domainDto.Id);
                return "Done successfully";
            }
            catch (NotFoundException)
            {
                throw new NotFoundException("دامنه غیرفعال یا حذف شده است.");
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                // Handle exceptions here
                throw new NotFoundException("سرور یافت نشد");
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
                Log.Error(ex.Message);
                // Handle exceptions here
                throw new NotFoundException("سرور یافت نشد");
            }
        }
        public async Task<string> DeleteTcpDnsAsync(string serverId)
        {
            try
            {
                if (serverId != null)
                {
                    var server = await _serverService.Detail(serverId);
                    await DeleteDnsRecord(server.ZoneId);
                    await ChangeTcp(server.Id.ToString());
                }


                return "Done successfully";
            }
            catch (NotFoundException)
            {
                throw new NotFoundException("سروری یافت نشد");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new NullException($"{ex.Message}");
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
            server.ZoneIdLastSynced = DateTime.UtcNow;
            server.ZoneId = server.ZoneId;
        }
        public void UpdateTcpServerConfig(ServerDTO server, string config, string newAddress, string newCnameValue)
        {
            var cnameValue1 = $"{newCnameValue}.{newAddress}";
            var cnameValue2 = $"{newCnameValue}.{newAddress}";
            var AddressValue = $"{newCnameValue}.{newAddress}";

            var configJson = JObject.Parse(config);
            UpdateAddressJsonValues(configJson, AddressValue, cnameValue1, cnameValue2);
            server.Config = configJson.ToString();
            server.IsNewDomain = true;
            server.DomainDateTime = DateTime.UtcNow;
            server.ZoneIdLastSynced = DateTime.UtcNow;
            server.ZoneId = GetDomain(newAddress).Result.ZoneId;
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
                Log.Error(ex.Message);
                throw new NotFoundException("سرور یافت نشد");
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
        public async Task ChangeDomain()
        {
            var serverIds = await _serverService.GetActiveIds();
            foreach (var id in serverIds)
            {
                await ChangeDomain(id.ToString());
            }
        }
        public async Task ChangeTcp()
        {
            var serverIds = await _serverService.GetActiveIds();
            foreach (var id in serverIds)
            {
                await ChangeTcp(id.ToString());
            }
        }
        public async Task<string> GenerateDnsAsync(string serverId, string expireMinuteOn)
        {
            try
            {
                await DeleteAndCheckDnsAsync(serverId, expireMinuteOn);
                await CreateDnsAsync(serverId);
                return "Done successfully";
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                Log.Error(ex.Message);
                throw new NotFoundException("سرور یافت نشد");
            }
        }

        public async Task<bool> DeleteDnsRecord(string cfZoneId)
        {
            var records = await _cloudflareApiClient.GetAllRecords(cfZoneId, CancellationToken.None);

            if (records.Count == 0)
                return true;

            foreach (var record in records)
            {
                await _cloudflareApiClient.DeleteCnameRecords(
                    cfZoneId,
                    record.id,
                    CancellationToken.None);
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
            var domain = domains.FirstOrDefault(x => !x.IsDeleted && x.IsActive && x.DomainName == name);
            return domain;
        }
        private async Task<DomainDTO> GetNewDomain()
        {
            var domains = await GetAll();
            if (domains.Count == 0)
            {
                return null;
            }
            return domains.FirstOrDefault(x => !x.IsDeleted && x.IsActive);
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
                throw new NotFoundException("سرور یافت نشد");
            }
        }
        private async Task<string> DeleteAndCheckDnsAsync(string serverId, string minuteTimeOn)
        {
            try
            {
                var cfZoneId = ServerDto(serverId, out var server, out var serverName);
                var cloudflare = _cloudflareApiClient;
                var recordsToDelete = await cloudflare.GetAllRecords(cfZoneId, CancellationToken.None);
                foreach (var record in recordsToDelete)
                {
                    if (!string.IsNullOrEmpty(record.content))
                    {
                        var modifiedDateTime = DateTime.Parse(record.content);
                        var expireTimeInMinutes = double.Parse(minuteTimeOn);
                        var expireTimeSpan = TimeSpan.FromMinutes(expireTimeInMinutes);

                        var currentTime = DateTime.Now;
                        var timeDifference = currentTime - modifiedDateTime;
                        if (timeDifference >= expireTimeSpan)
                        {
                            await cloudflare.DeleteCnameRecords(cfZoneId, record.id, CancellationToken.None);
                        }
                    }
                }
                return "Done successfully";
            }
            catch (Exception ex)
            {
                throw new NotFoundException("اشکال در اتصال به کلودفلر");
            }
        }

        private string ServerDto(string serverId, out ServerDTO server, out string serverName)
        {
            try
            {
                server = _serverService.Detail(serverId).GetAwaiter().GetResult();
                var config = server.Config;
                var jsonObject = JObject.Parse(config);
                var serverNameString = jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"]?.ToString()
                    .Split(".");
                serverName = $"{serverNameString?[1]}.{serverNameString?[2]}";
                return server.ZoneId;
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                throw new NotFoundException("سرور یافت نشد");
            }
        }


        private static void UpdateJsonValues(JObject config, string newServerName, string newHost)
        {
            const string serverNamePath = "outbounds[0].streamSettings.tlsSettings.serverName";
            const string hostPath = "outbounds[0].streamSettings.wsSettings.headers.Host";

            config.SelectToken(serverNamePath)?.Replace($"{newServerName}");
            config.SelectToken(hostPath)?.Replace($"{newHost}");
        }
        private static void UpdateAddressJsonValues(JObject config, string newAddress, string newServerName, string newHost)
        {
            const string serverNamePath = "outbounds[0].streamSettings.tlsSettings.serverName";
            const string hostPath = "outbounds[0].streamSettings.wsSettings.headers.Host";
            const string address = "outbounds[0].settings.vnext[0].address";
            config.SelectToken(address)?.Replace($"{newAddress}");
            config.SelectToken(serverNamePath)?.Replace($"{newServerName}");
            config.SelectToken(hostPath)?.Replace($"{newHost}");
        }



        private async Task<string> GetCloudflareZoneId(string cfDomain)
        {
            var cfClient1 = _cloudflareApiClient;
            var zones = await cfClient1.GetAllZonesAsync(cfDomain, CancellationToken.None);
            foreach (var zone1 in zones)
            {
                return zone1.id;
            }
            return null; // اگر زون پیدا نشد، null بازگردانید
        }

        private async Task UpdateDnsRecord(string cfZoneId, string newCnameValue, string cnameContent)
        {
            var cfClient = _cloudflareApiClient;
            var cnameRecord = await cfClient.GetCnameRecord(cfZoneId, CloudFlare.Client.Enumerators.DnsRecordType.Cname, CancellationToken.None);
            if (cnameRecord != null && await cfClient.IsExistCnameRecord(cfZoneId, CloudFlare.Client.Enumerators.DnsRecordType.Cname, CancellationToken.None))
            {
                await cfClient.UpdateDnsRecordAsync(cfZoneId, cnameRecord.id, newCnameValue.Trim(), cnameContent.Trim(), CancellationToken.None);
            }
            else
            {
                await cfClient.CreateDnsRecordAsync(cfZoneId, newCnameValue, cnameContent, CancellationToken.None);
            }
        }

        private async Task CreateDnsRecord(string cfZoneId, string newCnameValue, string cnameContent)
        {
            var cloudflare = _cloudflareApiClient;
            await cloudflare.CreateDnsRecordAsync(cfZoneId, newCnameValue.Trim(), cnameContent.Trim(), CancellationToken.None);
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
