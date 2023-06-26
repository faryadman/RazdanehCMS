using AutoMapper;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.IP;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;

namespace Project.Application.Features.Services
{
    public class IpService : IIpService
    {
        private readonly IIpRepository _ipRepository;
        private readonly IMapper _mapper;

        public IpService(IIpRepository jobRepository, IMapper mapper)
        {
            _ipRepository = jobRepository;
            _mapper = mapper;
        }
        public async Task<List<IpDTO>> List()
        {
            var ips = await _ipRepository.GetAll();
            var models = _mapper.Map<List<IpDTO>>(ips);
            return models;
        }

        public async Task<IpDTO> Detail(string ipName)
        {
            var ip = await _ipRepository.SingleOrDefaultAsync(i => i.Ip == ipName);
            var model = _mapper.Map<IpDTO>(ip);
            return model;
        }

        public async Task Delete(int id)
        {
            await _ipRepository.Delete(id);
        }

        public async Task Create(CreateIpDTO input)
        {
            var model = _mapper.Map<SaveIP>(input);
            await _ipRepository.Add(model);
        }
        public async Task Update(IpDTO input)
        {
            var model = _mapper.Map<SaveIP>(input);
            await _ipRepository.Update(model);
        }
    }
}
