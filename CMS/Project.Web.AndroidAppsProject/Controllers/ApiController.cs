using AutoMapper;
using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Project.Application.DTOs.ApiLog;
using Project.Application.DTOs.AppSetting;
using Project.Application.DTOs.Server;
using Project.Application.DTOs.ServerLog;
using Project.Application.Exceptions;
using Project.Application.Features.Interfaces;
using Project.Application.Features.Services;
using Project.Application.Responses;
using Project.Domain.Entities;
using Project.Persistence;
using Project.Persistence.Migrations;
using Project.Web.AndroidAppsProject.Dapper;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Project.Web.AndroidAppsProject.Controllers
{
    [Route("/[controller]/[action]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IAppSettingService _appSettingService;
        private readonly IGroupService _groupService;
        private readonly IServerService _serverService;
        private readonly IServerLogServie _serverLogServie;
        private readonly IOperatorIdentificationService _operatorIdentificationService;
        private readonly IApiLogService _apiLogService;
        private readonly IMapper _mapper;
        private readonly IDapperQueryService _dapperQueryService;
        private readonly ApplicationDbContext _context;

        public ApiController(IAppSettingService appSettingService, IGroupService groupService, IServerService serverService, IServerLogServie serverLogServie, ApplicationDbContext context, IOperatorIdentificationService operatorIdentificationService, IApiLogService apiLogService, IMapper mapper, IDapperQueryService dapperQueryService)
        {
            _appSettingService = appSettingService;
            _groupService = groupService;
            _serverService = serverService;
            _serverLogServie = serverLogServie;
            _context = context;
            _operatorIdentificationService = operatorIdentificationService;
            _apiLogService = apiLogService;
            _mapper = mapper;
            _dapperQueryService = dapperQueryService;
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
            //var operatorType = await _operatorIdentificationService.GetOperator(isp, Operator);

            //AppSettingDTO app = await _appSettingService.DetailByApiRoute(apiRoute);

            //if (string.IsNullOrWhiteSpace(app.GroupsThatAppIsJoinedIn))
            //    throw new BadRequestException("this app has no server");

            //string[] groups = app.GroupsThatAppIsJoinedIn.Split("_");

            //IEnumerable<Server> query = await _serverRepository.FindAsync(x =>
            //groups.Contains(x.GroupId.ToString())
            //&& x.IsAd == isAd
            //&& x.IsAvailable);

            //query = query.OrderByDescending(x => x.Id);

            //if (operatorType != Domain.Enums.Operator.Unknown)
            //{
            //    if (operatorType == Domain.Enums.Operator.Irancell)
            //    {
            //        query = query.Where(x => x.IsForIrancell).AsQueryable();
            //    }
            //    if (operatorType == Domain.Enums.Operator.HamraheAvval)
            //    {
            //        query = query.Where(x => x.IsForHamraheAvval).AsQueryable();
            //    }
            //}

            //var query = await _dapperQueryService.GetServerByApp(app.GroupsThatAppIsJoinedIn, false, operatorType);

            //int dataCount = query.Count();

            //if (query == null || dataCount == 0)
            //    throw new BadRequestException("this app has no server");

            //if (dataCount == 1)
            //{
            //    return new Response<ServerDTO>(_mapper.Map<ServerDTO>(query.FirstOrDefault())).ToJsonResult();
            //}

            //ApiLogDTO lastLog = await _apiLogService.GetLastLog(app.Id);


            //Server server = new Server();

            //if (app.SendRandomServer)
            //{
            //    int serverNotToReturnId = lastLog == null ? 0 : lastLog.ServerId;

            //    Random random = new Random();

            //    IEnumerable<Server> allowedServers = query.Where(x => x.Id != serverNotToReturnId);

            //    int index = random.Next(allowedServers.Count());

            //    server = allowedServers.ElementAt(index);
            //}
            //else
            //{
            //    if (lastLog == null)
            //    {
            //        Random random = new Random();
            //        int index = random.Next(query.Count());
            //        server = query.ElementAt(index);
            //    }
            //    else
            //    {
            //        int lastServerIndex = query.Select(x => x.Id).ToList().IndexOf(lastLog.ServerId);

            //        if (lastServerIndex == dataCount - 1)
            //        {
            //            server = query.FirstOrDefault();
            //        }
            //        else
            //        {
            //            int index = lastServerIndex == -1 ? 0 : lastServerIndex;
            //            server = query.ElementAt(index + 1);
            //        }
            //    }
            //}
            //await _apiLogService.Create(new ApiLogDTO
            //{
            //    AppSettingId = app.Id,
            //    ServerId = server.Id
            //});

            //ServerDTO dto = _mapper.Map<ServerDTO>(server);
            //dto.Config = dto.Config.Replace("@" + dto.ConfigKey, DateTime.Now.Ticks.ToString() + "." + dto.ConfigValue);

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
            await _serverLogServie.Create(input);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }
        [HttpPost]
        public async Task<IActionResult> FailedServerLog(AddServerLogDTO input)
        {
            var server = await _serverService.Detail(input.ServerId);
            input.Ip = server.Ip;
            input.ConnectionStatus = Domain.Enums.ConnectionStatus.Failed;
            await _serverLogServie.Create(input);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }
        [HttpGet]
        public async Task<IActionResult> ServerLogs(int serverId)
        {
            await _serverService.Detail(serverId);
            var data = await _serverLogServie.ListByServer(serverId);
            return new Response<List<ServerLogDTO>>(data).ToJsonResult();
        }
        [HttpGet]
        [NonAction]
        private IActionResult Test()
        {
            var logs = _context.ServerLogs.Include(x => x.Server).ToList();

            foreach (var item in logs)
            {
                item.Ip = item.Server.Ip;
            }
            _context.SaveChanges();
            return Ok(logs);
        }
    }
}
