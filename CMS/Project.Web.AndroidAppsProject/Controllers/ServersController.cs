using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _memoryCache;
        public ServersController(IServerService serverService, IMemoryCache memoryCache)
        {
            _serverService = serverService;
            _memoryCache = memoryCache;
        }

        public async Task<IActionResult> List(int? groupId, int? appId, bool isAd)
        {
            // تلاش برای خواندن اطلاعات از کش با استفاده از نام متد و پارامترهای ورودی به عنوان کلید
            string cacheKey = $"List_{groupId}_{appId}_{isAd}";
            if (_memoryCache.TryGetValue(cacheKey, out List<ServerDTO>? cachedServerList))
            {
                return new Response<List<ServerDTO>>(cachedServerList).ToJsonResult();
            }

            // اگر اطلاعات در کش موجود نباشند، آنها را از منبع اصلی (سرویس _serverService) دریافت می‌کنیم
            var data = await _serverService.GetWithFilter(groupId, appId, isAd);

            // ذخیره اطلاعات در کش با استفاده از نام متد و پارامترهای ورودی به عنوان کلید
            _memoryCache.Set(cacheKey, data);

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
