using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.AppSetting;
using Project.Application.DTOs.Group;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;

namespace Project.Application.Features.Services
{
    public class AppSettingService : IAppSettingService
    {
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IGroupService _groupService;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        public AppSettingService(IAppSettingRepository appSettingRepository, IMapper mapper, IGroupService groupService, IMemoryCache memoryCache)
        {
            _appSettingRepository = appSettingRepository;
            _mapper = mapper;
            _groupService = groupService;
            _memoryCache = memoryCache;
        }

        public async Task<List<MinimalAppSettingDTO>> GetAll()
        {
            var data = await _appSettingRepository.GetAll();
            return _mapper.Map<List<MinimalAppSettingDTO>>(data.OrderByDescending(x => x.UpdatedAt));
        }
        public async Task Create(CreateAppSettingDTO input)
        {
            var model = _mapper.Map<AppSetting>(input);
            await _appSettingRepository.Add(model);
        }
        public async Task<AppSettingDTO> Detail(int id)
        {
            var model = await _appSettingRepository.SingleOrDefaultAsync(x => x.Id == id);
            return _mapper.Map<AppSettingDTO>(model);
        }

        public async Task Edit(EditAppSettingDTO input, int id)
        {
            var model = await _appSettingRepository.SingleOrDefaultAsync(x => x.Id == id);
            _mapper.Map(input, model);
            await _appSettingRepository.Update(model);
            _memoryCache.Remove($"AppSetting_{input.ApiRoute}");
        }
        public async Task Delete(int id)
        {
            await _appSettingRepository.Delete(id);
        }

        public async Task<List<GroupsByAppDTO>> GroupsByApp(int id)
        {
            var app = await _appSettingRepository.SingleOrDefaultAsync(x => x.Id == id);
            var groups = await _groupService.GetAll();
            return groups.Select(item => new GroupsByAppDTO
            {
                Id = item.Id,
                Title = item.Title,
                IsAd = item.IsAd,
                DoTheyHaveRelation = !string.IsNullOrWhiteSpace(app.GroupsThatAppIsJoinedIn) && app.GroupsThatAppIsJoinedIn.Split("_").Contains(item.Id.ToString()),
            })
                .ToList();
        }

        public async Task UpdateAppGroups(int id, string groupIds)
        {
            var model = await _appSettingRepository.SingleOrDefaultAsync(x => x.Id == id && x.IsActive == true);
            model.GroupsThatAppIsJoinedIn = string.IsNullOrWhiteSpace(groupIds) ? "" : groupIds;
            await _appSettingRepository.Update(model);
            _memoryCache.Remove($"AppSetting_{model.ApiRoute}");
        }

        public async Task<AppSettingDTO> DetailByApiRoute(string apiRoute)
        {
            var model = await _appSettingRepository.SingleOrDefaultAsync(x => x.ApiRoute == apiRoute && x.IsActive == true);
            return _mapper.Map<AppSettingDTO>(model);
        }
    }
}
