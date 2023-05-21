using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.BlackList;
using Project.Application.DTOs.Server;
using Project.Application.Features.Interfaces;
using Project.Application.Responses;

namespace Project.Web.AndroidAppsProject.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ServersController : ControllerBase
    {
        private readonly IServerService _serverService;

        public ServersController(IServerService serverService)
        {
            _serverService = serverService;
        }

        public async Task<IActionResult> List(int? groupId, int? appId, bool isAd)
        {
            var data = await _serverService.GetWithFilter(groupId, appId, isAd);
            return new Response<List<ServerDTO>>(data).ToJsonResult();
        }
        public async Task<IActionResult> Create(CreateServerDTO input)
        {
            await _serverService.Create(input);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }
        public async Task<IActionResult> Detail(int id)
        {
            var server = await _serverService.Detail(id);
            return new Response<ServerDTO>(server).ToJsonResult();
        }
        public async Task<IActionResult> Edit(EditServerDTO input)
        {
            await _serverService.Edit(input);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }
        public async Task<IActionResult> Delete(int id)
        {
            await _serverService.Delete(id);
            return new Response<string>(ResponseStatus.Succeed).ToJsonResult();
        }
    }
}
