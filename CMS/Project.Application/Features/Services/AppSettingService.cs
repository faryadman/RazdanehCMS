using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.AppSetting;
using Project.Application.DTOs.Group;
using Project.Application.Exceptions;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Features.Services
{
    public class AppSettingService : IAppSettingService
    {
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IGroupService _groupService;
        private readonly IMapper _mapper;

        public AppSettingService(IAppSettingRepository appSettingRepository, IMapper mapper, IGroupService groupService)
        {
            _appSettingRepository = appSettingRepository;
            _mapper = mapper;
            _groupService = groupService;
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
            //var model = _mapper.Map<AppSetting>(input);
            await _appSettingRepository.Update(model);
        }
        public async Task Delete(int id)
        {
            await _appSettingRepository.Delete(id);
        }

        public async Task<List<GroupsByAppDTO>> GroupsByApp(int id)
        {
            var app = await _appSettingRepository.SingleOrDefaultAsync(x => x.Id == id);
            var groups = await _groupService.GetAll();
            var list = new List<GroupsByAppDTO>();
            foreach (var item in groups)
            {
                list.Add(new GroupsByAppDTO
                {
                    Id = item.Id,
                    Title = item.Title,
                    IsAd = item.IsAd,
                    DoTheyHaveRelation = string.IsNullOrWhiteSpace(app.GroupsThatAppIsJoinedIn) ? false : app.GroupsThatAppIsJoinedIn.Split("_").Contains(item.Id.ToString()),
                });
            }
            return list;
        }

        public async Task UpdateAppGroups(int id, string groupIds)
        {
            var model = await _appSettingRepository.SingleOrDefaultAsync(x => x.Id == id);
            model.GroupsThatAppIsJoinedIn = string.IsNullOrWhiteSpace(groupIds) ? "" : groupIds;
            await _appSettingRepository.Update(model);
        }

        public async Task<AppSettingDTO> DetailByApiRoute(string apiRoute)
        {
            var model = await _appSettingRepository.SingleOrDefaultAsync(x => x.ApiRoute == apiRoute);

            if (model == null)
                throw new NotFoundException();

            return _mapper.Map<AppSettingDTO>(model);
        }
    }
}
