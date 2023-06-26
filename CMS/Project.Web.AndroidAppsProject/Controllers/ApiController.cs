using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.AppSetting;
using Project.Application.DTOs.IP;
using Project.Application.DTOs.Server;
using Project.Application.DTOs.ServerLog;
using Project.Application.Features.Interfaces;
using Project.Application.Responses;
using Project.Persistence;
using Project.Web.AndroidAppsProject.Dapper;

namespace Project.Web.AndroidAppsProject.Controllers
{
    [Route("/[controller]/[action]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IAppSettingService _appSettingService;
        private readonly IGroupService _groupService;
        private readonly IServerService _serverService;
        private readonly IServerLogService _serverLogService;
        private readonly IOperatorIdentificationService _operatorIdentificationService;
        private readonly IApiLogService _apiLogService;
        private readonly IMapper _mapper;
        private readonly IDapperQueryService _dapperQueryService;
        private readonly IIpService _ipService;
        private readonly ApplicationDbContext _context;

        public ApiController(IAppSettingService appSettingService, IGroupService groupService, IServerService serverService, IServerLogService serverLogService, ApplicationDbContext context, IOperatorIdentificationService operatorIdentificationService, IApiLogService apiLogService, IMapper mapper, IDapperQueryService dapperQueryService, IIpService ipService)
        {
            _appSettingService = appSettingService;
            _groupService = groupService;
            _serverService = serverService;
            _serverLogService = serverLogService;
            _context = context;
            _operatorIdentificationService = operatorIdentificationService;
            _apiLogService = apiLogService;
            _mapper = mapper;
            _dapperQueryService = dapperQueryService;
            _ipService = ipService;
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
            var server = await _serverService.GetByApp(apiRoute, false, isp, Operator);
            return new Response<ServerDTO>(server).ToJsonResult();
        }

        [HttpGet]
        [Route("/[controller]/[action]/{apiRoute}/{isp}/{Operator}")]
        public async Task<IActionResult> GetAdServer(string apiRoute, string isp, string Operator)
        {
            var server = await _serverService.GetByApp(apiRoute, true, isp, Operator);
            return new Response<ServerDTO>(server).ToJsonResult();
        }
        [HttpPost]
        public async Task<IActionResult> SuccessServerLog(AddServerLogDTO input)
        {
            var server = await _serverService.Detail(input.ServerId);
            input.Ip = server.Ip;
            input.ConnectionStatus = Domain.Enums.ConnectionStatus.Successful;
            await _serverLogService.Create(input);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }
        [HttpPost]
        public async Task<IActionResult> FailedServerLog(AddServerLogDTO input)
        {
            var server = await _serverService.Detail(input.ServerId);
            input.Ip = server.Ip;
            input.ConnectionStatus = Domain.Enums.ConnectionStatus.Failed;
            await _serverLogService.Create(input);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }
        [HttpGet]
        public async Task<IActionResult> ServerLogs(int serverId)
        {
            await _serverService.Detail(serverId);
            var data = await _serverLogService.ListByServer(serverId);
            return new Response<List<ServerLogDTO>>(data).ToJsonResult();
        }


        [HttpPost]
        public async Task<IActionResult> AddIp(CreateIpDTO input)
        {
            var server = await _ipService.Detail(input.Ip);
            if (server != null)
            {
                await _ipService.Delete(server.Id);
            }
            await _ipService.Create(input);

            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }

        [HttpGet]
        public async Task<IActionResult> ListIp()
        {
            var data = await _ipService.List();
            return new Response<List<IpDTO>>(data).ToJsonResult();
        }
    }
}
