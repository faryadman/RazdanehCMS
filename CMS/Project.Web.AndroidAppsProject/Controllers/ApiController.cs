using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Project.Application.DTOs;
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

        public ApiController(IAppSettingService appSettingService, IServerService serverService,
            IServerLogService serverLogService, IIpService ipService, IMemoryCache memoryCache)
        {
            _appSettingService = appSettingService;
            _serverService = serverService;
            _serverLogService = serverLogService;
            _ipService = ipService;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        [Route("/[controller]/[action]/{apiRoute}/{isp}/{Operator}")]
        public async Task<IActionResult> GetGeneralApp(string apiRoute, string isp, string Operator)
        {
            var appSetting = await _appSettingService.DetailByApiRoute(apiRoute);
            var server = await _serverService.GetByApp(apiRoute, false, isp, Operator);
            var serverAd = await _serverService.GetByApp(apiRoute, true, isp, Operator);
            return new Response<GeneralServerDTO>(new GeneralServerDTO()
            {
                AppSettingDTO = appSetting,
                ServerDTO = server,
                ServerAdDTO = serverAd,
            }).ToJsonResult();
        }

        [HttpGet]
        [Route("/[controller]/[action]/{apiRoute}")]
        public async Task<IActionResult> AppSetting(string apiRoute)
        {
            var appSetting = await _appSettingService.DetailByApiRoute(apiRoute);
            return new Response<AppSettingDTO>(appSetting).ToJsonResult();
        }


        [HttpGet]
        [Route("/[controller]/[action]/{apiRoute}/{isp}/{Operator}")]
        public async Task<IActionResult> GetServer(string apiRoute, string isp, string Operator)
        {
            // اگر اطلاعات در کش نبود، آنها را از منبع اصلی دریافت کرده و در حافظه‌ی کش ذخیره می‌کنیم
            var server = await _serverService.GetByApp(apiRoute, false, isp, Operator);
            return new Response<ServerDTO>(server).ToJsonResult();
        }

        [HttpGet]
        [Route("/[controller]/[action]/{apiRoute}/{isp}/{Operator}")]
        public async Task<IActionResult> GetAdServer(string apiRoute, string isp, string Operator)
        {
            // اگر اطلاعات در کش نبود، آنها را از منبع اصلی دریافت کرده و در حافظه‌ی کش ذخیره می‌کنیم
            var server = await _serverService.GetByApp(apiRoute, true, isp, Operator);
            return new Response<ServerDTO>(server).ToJsonResult();
        }


        [HttpPost]
        public async Task<IActionResult> SuccessServerLog(AddServerLogDTO input)
        {
            // اگر اطلاعات در کش نبود، آنها را از منبع اصلی دریافت کرده و در حافظه‌ی کش ذخیره می‌کنیم
            await _serverService.SuccessServerLog(input);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }


        [HttpPost]
        public async Task<IActionResult> FailedServerLog(AddServerLogDTO input)
        {
            // اگر اطلاعات در کش نبود، آنها را از منبع اصلی دریافت کرده و در حافظه‌ی کش ذخیره می‌کنیم
            await _serverService.FailedServerLog(input);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }


        [HttpPost]
        public async Task<IActionResult> FailedServer(AddServerLogDTO input)
        {
            // اگر اطلاعات در کش نبود، آنها را از منبع اصلی دریافت کرده و در حافظه‌ی کش ذخیره می‌کنیم
            await _serverService.FailedServerLog(input);
            var server = await _serverService.GetByApp(input.ApiRoute, false, input.Isp, input.Operator);
            var serverAd = await _serverService.GetByApp(input.ApiRoute, true, input.Isp, input.Operator);
            return new Response<GeneralServerDTO>(new GeneralServerDTO()
            {
                AppSettingDTO = null,
                ServerDTO = server,
                ServerAdDTO = serverAd,
            }).ToJsonResult();
        }

        [HttpGet]
        public async Task<IActionResult> ServerLogs(int serverId)
        {
            // اگر اطلاعات در کش نبود، آنها را از منبع اصلی دریافت کرده و در حافظه‌ی کش ذخیره می‌کنیم
            var logs = await _serverLogService.ListByServer(serverId);
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
            await _ipService.Insert(new CreateIpDTO()
            {
                Tcp = tcpId.ToString(),
                Ip = clientIp,
                UserAgent = userAgent
            });
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }

        [HttpGet]
        public async Task<IActionResult> ListIp()
        {
            var list = await _ipService.List();
            return new Response<List<IpDTO>>(list).ToJsonResult();
        }
    }
}
