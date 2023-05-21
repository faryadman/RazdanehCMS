using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.BlackList;
using Project.Application.DTOs.Server;
using Project.Application.Features.Interfaces;
using Project.Application.Features.Services;
using Project.Application.Responses;

namespace Project.Web.AndroidAppsProject.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BlackListController : ControllerBase
    {
        private readonly IServerService _serverService;
        private readonly IBlackListService _blackListService;

        public BlackListController(IBlackListService blackListService, IServerService serverService)
        {
            _blackListService = blackListService;
            _serverService = serverService;
        }
        public async Task<IActionResult> Create([FromBody]CreateBlackListDTO input)
        {
            var server = await _serverService.Detail(input.ServerId);
            await _blackListService.Create(input.ServerId, input.Ip);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();

        }
        public async Task<IActionResult> GetData(int? serverId)
        {
            var data = await _blackListService.List(serverId);
            return new Response<List<BlackListDTO>>(data).ToJsonResult();
        }
    }
}
