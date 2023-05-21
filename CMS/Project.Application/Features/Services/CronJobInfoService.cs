using AutoMapper;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.CronJobInfo;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Features.Services
{
    public class CronJobInfoService : ICronJobInfoService
    {
        private readonly ICronJobInfoRepository _cronJobInfoRepository;
        private readonly IServerLogRepository _serverLogRepository;
        private readonly IMapper _mapper;

        public CronJobInfoService(ICronJobInfoRepository cronJobInfoRepository, IMapper mapper, IServerLogRepository serverLogRepository)
        {
            _cronJobInfoRepository = cronJobInfoRepository;
            _mapper = mapper;
            _serverLogRepository = serverLogRepository;
        }

        public async Task<CronJobInfoDTO> Get()
        {
            var model = await _cronJobInfoRepository.GetNoTracking(1);
            return _mapper.Map<CronJobInfoDTO>(model); ;
        }

        public async Task MassDelete()
        {
            var data = await _serverLogRepository.GetAll();

            foreach (var item in data)
            {
                await _serverLogRepository.Remove(item.Id);
            }
        }

        public async Task Update(UpdateCronJobInfoDTO input)
        {
            var data = await _cronJobInfoRepository.SingleOrDefaultAsync(x => x.Id == 1);
            data.Timer = input.Timer;
            await _cronJobInfoRepository.Update(data);
            //var model = _mapper.Map<CronJobInfo>(input);
            //await _cronJobInfoRepository.Add(model);
        }
    }
}
