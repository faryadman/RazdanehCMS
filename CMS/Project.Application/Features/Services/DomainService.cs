using AutoMapper;
using Project.Application.Jobs;
using Newtonsoft.Json.Linq;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.Domain;
using Project.Application.DTOs.Server;
using Project.Application.Exceptions;
using Project.Application.Features.Interfaces;
using Serilog;
using System.Security.Cryptography;

namespace Project.Application.Features.Services
{
    public class DomainService : IDomainService
    {
        private readonly IDomainRepository _domainRepository;
        private readonly JobExecutionScope executionScope;
        private CancellationToken ExecutionToken => executionScope?.CancellationToken ?? CancellationToken.None;
        private readonly IMapper _mapper;
        private readonly IServerService _serverService;
        private readonly ICloudflareApiClient _cloudflareApiClient;

        public DomainService(IMapper mapper, IDomainRepository domainRepository, IServerService serverService, ICloudflareApiClient cloudflareApiClient, JobExecutionScope executionScope = null)
        {
            _mapper = mapper;
            _domainRepository = domainRepository;
            _serverService = serverService;
            _cloudflareApiClient = cloudflareApiClient;
            this.executionScope = executionScope;
        }

        public async Task InitZoneId()
        {
            ExecutionToken.ThrowIfCancellationRequested();
            foreach (var domain in await _domainRepository.ListMissingZoneAsync(ExecutionToken))
                await ResolveZoneAsync(domain);
        }

        public Task CheckZoneId() => InitZoneId();

        public async Task<List<DomainDTO>> GetAll()
        {
            ExecutionToken.ThrowIfCancellationRequested();
            return _mapper.Map<List<DomainDTO>>(await _domainRepository.ListAsync(true, ExecutionToken));
        }

        // Legacy callers use both 0 and 1 for the active list; the inactive endpoint has its own query.
        public Task<List<DomainDTO>> GetByFilter(int filter) => GetAll();

        public async Task Create(CreateDomainDTO input)
        {
            ExecutionToken.ThrowIfCancellationRequested();
            var model = _mapper.Map<Domain.Entities.Domain>(input);
            ExecutionToken.ThrowIfCancellationRequested();
            await _domainRepository.Add(model);
        }

        public async Task<List<DomainDTO>> ListInactiveDomain()
        {
            ExecutionToken.ThrowIfCancellationRequested();
            return _mapper.Map<List<DomainDTO>>(await _domainRepository.ListAsync(false, ExecutionToken));
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
            ExecutionToken.ThrowIfCancellationRequested();
            var server = await GetRequiredServerAsync(serverId);
            var domain = await GetNewDomain();
            var cnameValue = await ChangeCnameDomain(server.CurrentDomainValue, domain.ZoneId);
            UpdateServerConfig(server, server.Config, cnameValue, domain.DomainName, domain.DomainName);
            server.ZoneId = domain.ZoneId;
            ExecutionToken.ThrowIfCancellationRequested();
            await _serverService.UpdateServer(server);
            await Delete(domain.Id);
            return "انجام شد.";
        }

        public async Task<string> ChangeTcp(string serverId)
        {
            ExecutionToken.ThrowIfCancellationRequested();
            var server = await GetRequiredServerAsync(serverId);
            var domain = await GetNewDomain();
            var cnameValue = await ChangeCnameDomain(server.CurrentDomainValue, domain.ZoneId);
            UpdateTcpServerConfig(server, server.Config, domain.DomainName, cnameValue);
            server.ZoneId = domain.ZoneId;
            ExecutionToken.ThrowIfCancellationRequested();
            await _serverService.UpdateServer(server);
            await Delete(domain.Id);
            return "انجام شد.";
        }

        private async Task<ServerDTO> GetRequiredServerAsync(string serverId) =>
            await _serverService.Detail(serverId) ?? throw new NotFoundException("سرور یافت نشد");

        public async Task<string> ChangeSubDomain(string serverId)
        {
            ExecutionToken.ThrowIfCancellationRequested();
            var server = await GetRequiredServerAsync(serverId);
            var config = JObject.Parse(server.Config);
            var serverName = config.SelectToken("outbounds[0].streamSettings.tlsSettings.serverName")?.Value<string>()?.TrimEnd('.');
            var hostName = config.SelectToken("outbounds[0].streamSettings.wsSettings.headers.Host")?.Value<string>()?.TrimEnd('.');
            if (string.IsNullOrWhiteSpace(serverName) || string.IsNullOrWhiteSpace(hostName))
                throw new BadRequestException("تنظیمات سرور نام دامنه و Host معتبر برای تغییر زیردامنه ندارد.");
            var domain = await _domainRepository.GetRegisteredByHostAsync(serverName, ExecutionToken)
                ?? throw new NotFoundException("دامنهٔ ثبت‌شدهٔ سرور پیدا نشد یا حذف شده است.");
            if (!string.Equals(hostName, domain.DomainName, StringComparison.OrdinalIgnoreCase)
                && !hostName.EndsWith("." + domain.DomainName, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("نام دامنه و Host سرور به یک دامنهٔ ثبت‌شده تعلق ندارند.");
            if (string.IsNullOrWhiteSpace(domain.ZoneId))
                domain = await ResolveZoneAsync(domain)
                    ?? throw new NotFoundException("دامنه هنگام واکشی زون غیرفعال یا حذف شده است.");
            var cnameValue = await ChangeCnameDomain(server.CurrentDomainValue, domain.ZoneId, true);
            UpdateServerConfig(server, server.Config, cnameValue, domain.DomainName, domain.DomainName);
            server.ZoneId = domain.ZoneId;
            ExecutionToken.ThrowIfCancellationRequested();
            await _serverService.UpdateServer(server);
            if (domain.IsActive) await Delete(domain.Id);
            return "Done successfully";
        }

        public async Task<string> DeleteDnsAsync(string serverId, string expireTime)
        {
            ExecutionToken.ThrowIfCancellationRequested();
            try
            {
                if (serverId != null)
                {
                    var (cfZoneId, server, serverName) = await ReadServerAsync(serverId);
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
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new NotFoundException("سرور یافت نشد");
            }
        }
        public async Task<string> DeleteTcpDnsAsync(string serverId)
        {
            ExecutionToken.ThrowIfCancellationRequested();
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
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new NullException($"{ex.Message}");
            }
        }

        private static void UpdateServerConfig(ServerDTO server, string config, string newCnameValue, string newServerName, string newHostName)
        {
            var cnameValue1 = $"{newCnameValue}.{newServerName}";
            var cnameValue2 = $"{newCnameValue}.{newHostName}";

            var configJson = JObject.Parse(config);
            UpdateJsonValues(configJson, cnameValue1, cnameValue2);

            server.Config = configJson.ToString();
            server.IsNewDomain = true;
            server.DomainDateTime = DateTime.UtcNow;
            server.ZoneIdLastSynced = DateTime.UtcNow;
        }
        private static void UpdateTcpServerConfig(ServerDTO server, string config, string newAddress, string newCnameValue)
        {
            var address = $"{newCnameValue}.{newAddress}";
            var configJson = JObject.Parse(config);
            UpdateAddressJsonValues(configJson, address);
            server.Config = configJson.ToString();
            server.IsNewDomain = true;
            server.DomainDateTime = DateTime.UtcNow;
            server.ZoneIdLastSynced = DateTime.UtcNow;
        }
        public async Task<string> CreateDnsAsync(string serverId)
        {
            ExecutionToken.ThrowIfCancellationRequested();
            try
            {
                var (zoneId, server, serverName) = await ReadServerAsync(serverId);
                var cnameValue = await ChangeCnameDomain(server.CurrentDomainValue, zoneId, true);
                UpdateServerConfig(server, server.Config, cnameValue, serverName, serverName);
                ExecutionToken.ThrowIfCancellationRequested();
                await _serverService.UpdateServer(server);
                return "Done successfully";
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new NotFoundException("سرور یافت نشد");
            }
        }
        public async Task ChangeSubDomain()
        {
            ExecutionToken.ThrowIfCancellationRequested();
            var serverIds = await _serverService.GetActiveIds();
            foreach (var id in serverIds)
            {
                await ChangeSubDomain(id.ToString());
            }
        }
        public async Task ChangeDomain()
        {
            ExecutionToken.ThrowIfCancellationRequested();
            var serverIds = await _serverService.GetActiveIds();
            foreach (var id in serverIds)
            {
                await ChangeDomain(id.ToString());
            }
        }
        public async Task ChangeTcp()
        {
            ExecutionToken.ThrowIfCancellationRequested();
            var serverIds = await _serverService.GetActiveIds();
            foreach (var id in serverIds)
            {
                await ChangeTcp(id.ToString());
            }
        }
        public async Task<string> GenerateDnsAsync(string serverId, string expireMinuteOn)
        {
            ExecutionToken.ThrowIfCancellationRequested();
            try
            {
                await DeleteAndCheckDnsAsync(serverId, expireMinuteOn);
                await CreateDnsAsync(serverId);
                return "Done successfully";
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new NotFoundException("سرور یافت نشد");
            }
        }

        public async Task<bool> DeleteDnsRecord(string cfZoneId)
        {
            ExecutionToken.ThrowIfCancellationRequested();
            var records = await _cloudflareApiClient.GetAllRecords(cfZoneId, ExecutionToken);

            if (records.Count == 0)
                return true;

            foreach (var record in records)
            {
                await _cloudflareApiClient.DeleteCnameRecords(
                    cfZoneId,
                    record.id,
                    ExecutionToken);
            }

            return true;
        }
        public async Task Delete(int id)
        {
            ExecutionToken.ThrowIfCancellationRequested();
            await _domainRepository.SetInactiveAsync(id, ExecutionToken);
        }

        public async Task Remove()
        {
            ExecutionToken.ThrowIfCancellationRequested();
            await _domainRepository.DeleteInactiveAsync(ExecutionToken);
        }

        private async Task<DomainDTO> GetNewDomain()
        {
            var domain = await _domainRepository.GetNextActiveAsync(ExecutionToken)
                ?? throw new NotFoundException("دامنهٔ فعال و قابل‌استفاده‌ای موجود نیست؛ فهرست دامنه‌ها را بررسی کنید.");
            if (string.IsNullOrWhiteSpace(domain.ZoneId))
                domain = await ResolveZoneAsync(domain)
                    ?? throw new NotFoundException("دامنه هنگام واکشی زون غیرفعال یا حذف شده است.");
            return _mapper.Map<DomainDTO>(domain);
        }

        private async Task<Domain.Entities.Domain> ResolveZoneAsync(Domain.Entities.Domain domain)
        {
            ExecutionToken.ThrowIfCancellationRequested();
            var zoneId = await GetCloudflareZoneId(domain.DomainName);
            if (string.IsNullOrWhiteSpace(zoneId))
                throw new NotFoundException($"شناسهٔ زون برای دامنهٔ {domain.DomainName} در حساب Cloudflare پیدا نشد.");
            if (await _domainRepository.TrySetZoneAsync(domain.Id, domain.DomainName, zoneId, ExecutionToken))
            {
                domain.ZoneId = zoneId;
                return domain;
            }
            // A concurrent admin edit must win over this lookup; never restore a disabled domain or overwrite its zone.
            var current = await _domainRepository.GetActiveByIdAsync(domain.Id, ExecutionToken);
            if (current == null) return null;
            if (string.IsNullOrWhiteSpace(current.ZoneId))
                throw new NotFoundException("تنظیمات دامنه هنگام واکشی زون تغییر کرده است؛ دوباره بررسی کنید.");
            return current;
        }

        private async Task<string> ChangeCnameDomain(string currentDomain, string zoneId, bool isActiveSubDomain = false)
        {
            ExecutionToken.ThrowIfCancellationRequested();
            var cnameValue = RandomNumberGenerator.GetString("abcdefghijklmnopqrstuvwxyz", 5);
            if (isActiveSubDomain) await CreateDnsRecord(zoneId, cnameValue, currentDomain);
            else await UpdateDnsRecord(zoneId, cnameValue, currentDomain);
            return cnameValue;
        }

        private async Task<string> DeleteAndCheckDnsAsync(string serverId, string minuteTimeOn)
        {
            try
            {
                var (cfZoneId, server, serverName) = await ReadServerAsync(serverId);
                var cloudflare = _cloudflareApiClient;
                var recordsToDelete = await cloudflare.GetAllRecords(cfZoneId, ExecutionToken);
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
                            await cloudflare.DeleteCnameRecords(cfZoneId, record.id, ExecutionToken);
                        }
                    }
                }
                return "Done successfully";
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception)
            {
                throw new NotFoundException("اشکال در اتصال به کلودفلر");
            }
        }

        private async Task<(string ZoneId, ServerDTO Server, string ServerName)> ReadServerAsync(string serverId)
        {
            try
            {
                var server = await _serverService.Detail(serverId);
                ExecutionToken.ThrowIfCancellationRequested();
                var config = server.Config;
                var jsonObject = JObject.Parse(config);
                var serverNameString = jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"]?.ToString()
                    .Split(".");
                var serverName = $"{serverNameString?[1]}.{serverNameString?[2]}";
                return (server.ZoneId, server, serverName);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception)
            {
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
        private static void UpdateAddressJsonValues(JObject config, string newAddress)
        {
            const string address = "outbounds[0].settings.vnext[0].address";
            config.SelectToken(address)?.Replace(newAddress);
            UpdateJsonValues(config, newAddress, newAddress);
        }



        private async Task<string> GetCloudflareZoneId(string domainName)
        {
            var zones = await _cloudflareApiClient.GetAllZonesAsync(domainName, ExecutionToken);
            return zones.FirstOrDefault(zone => string.Equals(zone.name, domainName, StringComparison.OrdinalIgnoreCase))?.id;
        }

        private async Task UpdateDnsRecord(string zoneId, string newCnameValue, string cnameContent)
        {
            var record = await _cloudflareApiClient.GetCnameRecord(zoneId, CloudFlare.Client.Enumerators.DnsRecordType.Cname, ExecutionToken);
            if (record != null)
                await _cloudflareApiClient.UpdateDnsRecordAsync(zoneId, record.id, newCnameValue.Trim(), cnameContent.Trim(), ExecutionToken);
            else
                await _cloudflareApiClient.CreateDnsRecordAsync(zoneId, newCnameValue, cnameContent, ExecutionToken);
        }

        private Task CreateDnsRecord(string zoneId, string newCnameValue, string cnameContent) =>
            _cloudflareApiClient.CreateDnsRecordAsync(zoneId, newCnameValue.Trim(), cnameContent.Trim(), ExecutionToken);
    }
}
