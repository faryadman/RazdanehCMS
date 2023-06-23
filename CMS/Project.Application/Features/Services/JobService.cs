using AutoMapper;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.Job;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;

namespace Project.Application.Features.Services
{
    public class JobService : IJobService
    {

        private readonly IJobRepository _jobRepository;
        private readonly IMapper _mapper;

        public JobService(IJobRepository jobRepository, IMapper mapper)
        {
            _jobRepository = jobRepository;
            _mapper = mapper;
        }
        public async Task<List<JobDTO>> List()
        {
            var jobs = await _jobRepository.GetAll();
            var models = _mapper.Map<List<JobDTO>>(jobs);
            return models;
        }
        public Task<JobDTO> LastDetail()
        {
            var job = _jobRepository.GetAllQueryable().OrderBy(x => x.Id).LastOrDefault();
            var model = _mapper.Map<JobDTO>(job);
            return Task.FromResult(model);
        }
        public async Task<JobDTO> Detail(int id)
        {
            var job = await _jobRepository.GetNoTracking(id);
            var model = _mapper.Map<JobDTO>(job);
            return model;
        }

        public async Task Delete(int id)
        {
            await _jobRepository.Delete(id);
        }

        public async Task CreateJob(CreateJobDTO input)
        {
            var model = _mapper.Map<Job>(input);
            await _jobRepository.Add(model);
        }
    }
}
