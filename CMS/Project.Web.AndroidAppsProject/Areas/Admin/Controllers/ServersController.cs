using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Project.Application.DTOs.Server;
using Project.Application.Features.Interfaces;

namespace Project.Web.AndroidAppsProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class ServersController : Controller
    {
        private readonly IServerService _serverService;
        private readonly IServerLogServie _serverLogServie;
        private readonly IBlackListService _blackListService;
        private readonly ICronJobInfoService _cronJobInfoService;
        private readonly IDomainService _domainService;

        public ServersController(IServerService serverService, IBlackListService blackListService, IServerLogServie serverLogServie, ICronJobInfoService cronJobInfoService, IDomainService domainService)
        {
            _serverService = serverService;
            _blackListService = blackListService;
            _serverLogServie = serverLogServie;
            _cronJobInfoService = cronJobInfoService;
            _domainService = domainService;
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
        public IActionResult Logs(int serverId)
        {
            ViewBag.ServerId = serverId;
            return View();
        }
        public async Task<IActionResult> ChangeDomain(int serverId)
        {
            ViewBag.ServerId = serverId;
            var domains = await _domainService.GetAll();
            if (domains == null) return Json(new { status = "2", message = "domain don't exist!" });
            var server = await _serverService.Detail(serverId);
            var config = server.Config;
            var currentDomainValue = domains?[0].DomainName;


            JObject jsonObject = JObject.Parse(config);
            // تغییر مقدار serverName
            var serverNameJson = jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"];
            var serverNameSplit = serverNameJson?.ToString().Split(".");
            jsonObject["outbounds"]![0]!["streamSettings"]!["tlsSettings"]!["serverName"] = $"{serverNameSplit?[0]}.{currentDomainValue}";

            // تغییر مقدار Host
            var hostJson = jsonObject["outbounds"]![0]!["streamSettings"]!["wsSettings"]!["headers"]!["Host"];
            var hostSplit = hostJson?.ToString().Split(".");
            jsonObject["outbounds"]![0]!["streamSettings"]!["wsSettings"]!["headers"]!["Host"] = $"{hostSplit?[0]}.{currentDomainValue}";

            var updatedJsonString = jsonObject.ToString();
            server.Config = updatedJsonString;

            await _serverService.Edit(new EditServerDTO()
            {
                Config = server.Config,
                ServerName = server.ServerName,
                CurrentDomainValue = currentDomainValue,
                ConfigValue = server.ConfigValue,
                ConfigKey = server.ConfigKey,
                Ip = server.Ip,
                IsForHamraheAvval = server.IsForHamraheAvval,
                IsForIrancell = server.IsForIrancell,
                ItemId = server.Id,
                Location = server.Location
            });
            await _domainService.Delete(domains[0].Id);



            return RedirectToAction("Index");
        }
        [Route("/admin/[controller]/Logs/list")]
        public async Task<IActionResult> LogsList(int serverId)
        {
            var data = await _serverLogServie.ListByServer(serverId);
            return Json(data);
        }

        public IActionResult AllLogs()
        {
            return View();
        }
        [Route("/admin/[controller]/Logs/getAllLogsStatistics")]
        public async Task<IActionResult> GetAllLogsStatistics()
        {
            var data = await _serverLogServie.GetAllLogsStatistics();
            return Json(data);
        }
    }
}
