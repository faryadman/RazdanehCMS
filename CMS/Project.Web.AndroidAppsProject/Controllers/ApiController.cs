using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Project.Application.DTOs.AppSetting;
using Project.Application.DTOs.IP;
using Project.Application.DTOs.Server;
using Project.Application.DTOs.ServerLog;
using Project.Application.Features.Interfaces;
using Project.Application.Responses;

namespace Project.Web.AndroidAppsProject.Controllers
{
    [Route("/[controller]/[action]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IAppSettingService _appSettingService;
        private readonly IServerService _serverService;
        private readonly IServerLogService _serverLogService;
        private readonly IIpService _ipService;
        private readonly IMemoryCache _memoryCache;

        public ApiController(IAppSettingService appSettingService, IServerService serverService, IServerLogService serverLogService, IIpService ipService, IMemoryCache memoryCache)
        {
            _appSettingService = appSettingService;
            _serverService = serverService;
            _serverLogService = serverLogService;
            _ipService = ipService;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        [Route("/[controller]/[action]/{apiRoute}")]
        public async Task<IActionResult> AppSetting(string apiRoute)
        {
            if (_memoryCache.TryGetValue($"AppSetting_{apiRoute}", out AppSettingDTO? cachedAppSetting))
            {
                return new Response<AppSettingDTO>(cachedAppSetting).ToJsonResult();
            }

            // اگر اطلاعات در کش نبود، آنها را از منبع اصلی (سرویس _appSettingService) دریافت می‌کنیم
            var appSetting = await _appSettingService.DetailByApiRoute(apiRoute);

            // ذخیره اطلاعات در کش
            _memoryCache.Set($"AppSetting_{apiRoute}", appSetting);

            return new Response<AppSettingDTO>(appSetting).ToJsonResult();
        }

        [HttpGet]
        [Route("/[controller]/[action]/{apiRoute}/{isp}/{Operator}")]
        public async Task<IActionResult> GetServer(string apiRoute, string isp, string Operator)
        {
            if (_memoryCache.TryGetValue($"GetServer_{apiRoute}", out ServerDTO? cachedGetServer))
            {
                return new Response<ServerDTO>(cachedGetServer).ToJsonResult();
            }
            var server = await _serverService.GetByApp(apiRoute, false, isp, Operator);
            // ذخیره اطلاعات در کش
            _memoryCache.Set($"GetServer_{apiRoute}", server);

            return new Response<ServerDTO>(server).ToJsonResult();
        }

        [HttpGet]
        [Route("/[controller]/[action]/{apiRoute}/{isp}/{Operator}")]
        public async Task<IActionResult> GetAdServer(string apiRoute, string isp, string Operator)
        {

            if (_memoryCache.TryGetValue($"GetAdServer_{apiRoute}", out ServerDTO? cachedGetAdServer))
            {
                return new Response<ServerDTO>(cachedGetAdServer).ToJsonResult();
            }
            var server = await _serverService.GetByApp(apiRoute, true, isp, Operator);
            // ذخیره اطلاعات در کش
            _memoryCache.Set($"GetAdServer_{apiRoute}", server);
            return new Response<ServerDTO>(server).ToJsonResult();
        }


        [HttpPost]
        public async Task<IActionResult> SuccessServerLog(AddServerLogDTO input)
        {
            if (_memoryCache.TryGetValue($"SuccessServerLog_{input.ServerId}", out AddServerLogDTO? cachedSuccessServerLog) &&
                cachedSuccessServerLog is { ConnectionStatus: Domain.Enums.ConnectionStatus.Successful })
            {
                return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
            }

            var server = await _serverService.Detail(input.ServerId);
            input.Ip = server.Ip;
            input.ConnectionStatus = Domain.Enums.ConnectionStatus.Successful;

            // ذخیره‌ی اطلاعات در حافظه‌ی کش
            _memoryCache.Set($"SuccessServerLog_{input.ServerId}", input);

            await _serverLogService.Create(input);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }


        [HttpPost]
        public async Task<IActionResult> FailedServerLog(AddServerLogDTO input)
        {
            if (_memoryCache.TryGetValue($"FailedServerLog_{input.ServerId}", out AddServerLogDTO? cachedFailedServerLog) &&
                cachedFailedServerLog is { ConnectionStatus: Domain.Enums.ConnectionStatus.Failed })
            {
                return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
            }

            var server = await _serverService.Detail(input.ServerId);
            input.Ip = server.Ip;
            input.ConnectionStatus = Domain.Enums.ConnectionStatus.Failed;

            // ذخیره‌ی اطلاعات در حافظه‌ی کش
            _memoryCache.Set($"FailedServerLog_{input.ServerId}", input);

            await _serverLogService.Create(input);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }

        [HttpGet]
        public async Task<IActionResult> ServerLogs(int serverId)
        {
            var cacheKey = $"ServerLogs_{serverId}";

            // ابتدا تلاش می‌کنیم اطلاعات را از حافظه‌ی کش بخوانیم
            if (_memoryCache.TryGetValue(cacheKey, out List<ServerLogDTO>? cachedLogs))
            {
                return new Response<List<ServerLogDTO>?>(cachedLogs).ToJsonResult();
            }

            // اگر اطلاعات در کش نبود، آنها را از منبع اصلی دریافت کرده و در حافظه‌ی کش ذخیره می‌کنیم
            var logs = await _serverLogService.ListByServer(serverId);

            _memoryCache.Set(cacheKey, logs);

            return new Response<List<ServerLogDTO>>(logs).ToJsonResult();
        }



        [HttpGet]
        public async Task<IActionResult> AddIp([FromQuery] int tcp)
        {
            if (!int.TryParse(tcp.ToString(), out var tcpId))
            {
                return new JsonResult(new { status = 3, message = "Invalid TCP" });
            }
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var server = await _ipService.Detail(clientIp);
            if (server != null)
            {
                await _ipService.Delete(server.Id);
            }

            // ابتدا تلاش می‌کنیم اطلاعات را از حافظه‌ی کش بخوانیم
            if (!_memoryCache.TryGetValue($"AddIp_{clientIp}", out CreateIpDTO? cachedIpData))
            {
                // اگر اطلاعات در کش نبود، آنها را از منبع اصلی (درخواست HTTP) دریافت می‌کنیم
                cachedIpData = new CreateIpDTO
                {
                    Ip = clientIp,
                    Tcp = tcpId.ToString(),
                    UserAgent = userAgent
                };

                // سپس اطلاعات را در کش ذخیره می‌کنیم
                _memoryCache.Set($"AddIp_{clientIp}", cachedIpData);
            }

            await _ipService.Create(cachedIpData);

            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }

        [HttpGet]
        public async Task<IActionResult> ListIp()
        {
            // ابتدا تلاش می‌کنیم اطلاعات را از حافظه‌ی کش بخوانیم
            if (_memoryCache.TryGetValue("ListIp", out List<IpDTO> cachedIpList))
                return new Response<List<IpDTO>>(cachedIpList).ToJsonResult();
            // اگر اطلاعات در کش نبود، آنها را از منبع اصلی (سرویس _ipService) دریافت می‌کنیم
            cachedIpList = await _ipService.List();

            // سپس اطلاعات را در کش ذخیره می‌کنیم
            _memoryCache.Set("ListIp", cachedIpList);

            return new Response<List<IpDTO>>(cachedIpList).ToJsonResult();
        }
    }
}
