using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.ApiLog;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;

namespace Project.Application.Features.Services
{
    public class ApiLogService : IApiLogService
    {
        private readonly IApiLogRepository _apiLogRepository;
        private readonly IMapper _mapper;

        public ApiLogService(IApiLogRepository apiLogRepository, IMapper mapper)
        {
            _apiLogRepository = apiLogRepository;
            _mapper = mapper;
        }

        public async Task Create(ApiLogDTO input)
        {
            var model = _mapper.Map<ApiLog>(input);
            await _apiLogRepository.Add(model);
        }

        public async Task<ApiLogDTO> GetLastLog(int appSettingId)
        {
            var query = _apiLogRepository.FindQueryable(x => x.AppSettingId == appSettingId).OrderByDescending(x => x.Id);
            var model = await query.FirstOrDefaultAsync();
            return _mapper.Map<ApiLogDTO>(model);
        }

        public async Task DeleteApiLog(int count = 100000)
        {
            var list = await _apiLogRepository.GetAll();
            foreach (var apiLog in list)
            {
                await _apiLogRepository.Delete(apiLog.Id);
            }
        }


    }
}
