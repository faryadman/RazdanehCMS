using CloudFlare.NET;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Project.Application.DTOs.Server;
using Project.Application.Extensions;
using Project.Application.Features;
using Project.Application.Features.Interfaces;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class ServersController : Controller
    {
        private readonly IServerService _serverService;
        private readonly IServerLogService _serverLogService;
        private readonly IBlackListService _blackListService;
        private readonly IDomainService _domainService;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;

        public ServersController(IServerService serverService, IBlackListService blackListService, IServerLogService serverLogService, IDomainService domainService, IMemoryCache memoryCache, IConfiguration configuration)

        {
            _serverService = serverService;
            _blackListService = blackListService;
            _serverLogService = serverLogService;
            _domainService = domainService;
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public IActionResult Index(int? groupId, int? appId)
        {
            ViewBag.GroupId = groupId;
            ViewBag.AppId = appId;
            return View();
        }
        public IActionResult AdIndex(int? groupId, int? appId)
        {
            ViewBag.GroupId = groupId;
            ViewBag.AppId = appId;
            return View();
        }
        public async Task<IActionResult> List(int? groupId, int? appId, bool isAd, int filter)
        {
            var data = await _serverService.GetWithFilter(groupId, appId, isAd, filter);
            return Json(data);
        }
        public async Task<IActionResult> Create(CreateServerDTO input)
        {
            await _serverService.Create(input);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> CreateAd(CreateServerDTO input)
        {
            input.IsAd = true;
            await _serverService.Create(input);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> Detail(int id)
        {
            var server = await _serverService.Detail(id);
            return Json(server);
        }
        public async Task<IActionResult> Edit(EditServerDTO input)
        {
            await _serverService.Edit(input);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> EditAd(EditServerDTO input)
        {
            await _serverService.Edit(input);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> ToggleIsAvailableInput(int id)
        {
            await _serverService.ToggleIsAvailableInput(id);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> Delete(int id)
        {
            await _serverService.Delete(id);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> Duplicate(int id)
        {
            await _serverService.Duplicate(id);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> MassDelete(string ids)
        {
            foreach (var item in ids.Split("_"))
            {
                await _serverService.Delete(int.Parse(item));
            }
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> AddToBlackList(int id)
        {
            var server = await _serverService.Detail(id);
            await _blackListService.Create(server.Id, server.Ip);
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> BlackList(int? id)
        {
            ViewBag.Servers = await _serverService.GetAllIds();
            ViewBag.ServerId = id;
            return View();
        }
        [Route("/admin/[controller]/blackList/create")]
        public async Task<IActionResult> Create(string ip, int serverId)
        {
            var server = await _serverService.Detail(serverId);
            await _blackListService.Create(serverId, ip);
            return Json(new { status = "1", message = "done successfully" });
        }
        [Route("/admin/[controller]/blackList/getData")]
        public async Task<IActionResult> GetBlackListData(int? id)
        {
            ViewBag.ServerId = id;
            var data = await _blackListService.List(id);
            return Json(data);
        }
        [Route("/admin/[controller]/blackList/delete")]
        public async Task<IActionResult> DeleteBlackList(int blackListId)
        {
            await _blackListService.Delete(blackListId);
            return Json(new { status = "1", message = "done successfully" });
        }
        [Route("/admin/[controller]/blackList/MassDelete")]
        public async Task<IActionResult> BlackListMassDelete(string ids)
        {
            foreach (var item in ids.Split("_"))
            {
                await _blackListService.Delete(int.Parse(item));
            }
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> RefreshSubdomain(string ids)
        {
            foreach (var item in ids.Split("_"))
            {
                await ChangeSubDomain(item);
            }
            return Json(new { status = "1", message = "done successfully" });
        }
        public async Task<IActionResult> RefreshDomain(string ids)
        {
            string apiKey = _configuration["CloudflareData:ApiKey"];
            string email = _configuration["CloudflareData:Email"];


            // Create a list to store the tasks
            var changeDomainTasks = new List<Task>();

            // Iterate through the IDs and start a task for each
            foreach (var item in ids.Split("_"))
            {
                // Start a task for each ChangeDomain operation
                var task = _domainService.ChangeDomain(item, email, apiKey);
                changeDomainTasks.Add(task);
            }

            try
            {
                // Wait for all tasks to complete
                await Task.WhenAll(changeDomainTasks);

                // All tasks completed successfully
                return Json(new { status = "1", message = "done successfully" });
            }
            catch (Exception ex)
            {
                // Handle exceptions if any of the tasks fail
                return Json(new { status = "0", message = $"An error occurred: {ex.Message}" });
            }
        }
        public IActionResult Logs(int serverId)
        {
            ViewBag.ServerId = serverId;
            return View();
        }

        public async Task<IActionResult> ChangeServerAddressInput(string address)
        {
            var listServer = await _serverService.GetAllIds();
            foreach (var id in listServer)
            {
                var server = await _serverService.Detail(id);
                var config = server.Config;
                var updatedJsonString = _domainService.SetServerAddressStrings(config, address).ToString();
                server.Config = updatedJsonString;
                await _serverService.UpdateServer(server);
            }
            return Json(new { status = "1", message = "done successfully" });
        }


        public async Task<IActionResult> ChangeDomain(string id)
        {
            var apiKey = _configuration["CloudflareData:ApiKey"];
            var email = _configuration["CloudflareData:Email"];
            return Json(new { status = "1", message = await _domainService.ChangeDomain(id, email, apiKey) });
        }

        public async Task<IActionResult> ChangeSubDomain(string id)
        {
            var server = await _serverService.Detail(id);
            var config = server.Config;
            var jsonObject = JObject.Parse(config);
            var serverNameString = jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"]?.ToString().Split(".");
            var hostString = jsonObject["outbounds"]![0]!["streamSettings"]!["wsSettings"]!["headers"]!["Host"]?.ToString().Split(".");
            var subName = GenerateWordExtention.GenerateWords(4)[0];
            var subServerName = GenerateWordExtention.GenerateWords(3)[0];
            var serverName = $"{subServerName}.{serverNameString?[1]}.{serverNameString?[2]}";
            var host = $"{subName}.{hostString?[1]}.{hostString?[2]}";
            jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"] = serverName;
            jsonObject["outbounds"]![0]!["streamSettings"]!["wsSettings"]!["headers"]!["Host"] = host;
            var updatedJsonString = jsonObject.ToString();
            server.Config = updatedJsonString;
            await _serverService.Edit(new EditServerDTO()
            {
                Config = server.Config,
                ServerName = server.ServerName,
                ConfigValue = server.ConfigValue,
                ConfigKey = server.ConfigKey,
                Ip = server.Ip,
                IsForHamraheAvval = server.IsForHamraheAvval,
                IsForIrancell = server.IsForIrancell,
                ItemId = server.Id,
                Location = server.Location,
                CurrentDomainValue = server.CurrentDomainValue,
                IsNewDomain = true,
                DomainDateTime = DateTime.UtcNow
            });
            var currentDomainValue = serverNameString;
            // Your Cloudflare API credentials
            var cfEmail = _configuration["CloudflareData:Email"];
            var cfApiKey = _configuration["CloudflareData:ApiKey"];

            // Your Cloudflare zone ID and domain name
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
            // Get the list of DNS records in the zone
            var dnsRecords = await cfClient.GetDnsRecordsAsync(cfZoneId);

            // Find the CNAME record based on its name
            var cnameRecord = dnsRecords.Result.FirstOrDefault(record => record.Type == DnsRecordType.CNAME);
            var cloudflare = new CloudflareApiClient();
            if (cnameRecord != null)
            {
                // Update the CNAME record with the new value
                await cloudflare.UpdateDnsRecordAsync(cfZoneId, cnameRecord.Id, host, server.CurrentDomainValue, cfApiKey, cfEmail);
            }
            else
            {
                // Create the CNAME record with the new value
                await cloudflare.CreateDnsRecordAsync(cfZoneId, host, server.CurrentDomainValue, cfApiKey, cfEmail); ;
            }

            return Json(new { status = "1", message = "Done Subdomain !" });
        }

        public async Task<IActionResult> CreateDnsRecord(string id)
        {
            var apiKey = _configuration["CloudflareData:ApiKey"];
            var email = _configuration["CloudflareData:Email"];
            return Json(new { status = "1", message = await _domainService.CreateDnsAsync(id, email, apiKey) });
        }
        public async Task<IActionResult> DeleteDnsRecord(string id)
        {
            var apiKey = _configuration["CloudflareData:ApiKey"];
            var email = _configuration["CloudflareData:Email"];
            return Json(new { status = "1", message = await _domainService.DeleteDnsAsync(id, email, apiKey) });
        }
        [Route("/admin/[controller]/Logs/list")]
        public async Task<IActionResult> LogsList(int serverId)
        {
            var data = await _serverLogService.ListByServer(serverId);
            return Json(data);
        }


        public IActionResult AllLogs()
        {
            return View();
        }
        [Route("/admin/[controller]/Logs/getAllLogsStatistics")]
        public IActionResult GetAllLogsStatistics()
        {
            var data = _serverLogService.GetAllLogsStatistics();
            return Json(data);
        }
    }
}
