using AutoMapper;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.IpConfig;
using Project.Application.Features.Interfaces;
using Project.Domain.Entities;

namespace Project.Application.Features.Services
{
    public class IpConfigService : IIPConfigService
    {
        private readonly IIpConfigRepository _ipConfigRepository;
        private readonly IDomainService _domainService;
        private readonly IServerService _serverService;
        private readonly IMapper _mapper;

        public IpConfigService(IDomainService domainService, IIpConfigRepository ipConfigRepository, IMapper mapper, IServerService serverService)
        {
            _domainService = domainService;
            _ipConfigRepository = ipConfigRepository;
            _mapper = mapper;
            _serverService = serverService;
        }

        public IPConfigEntity Get(int? id)
        {
            return id is null
                ? _ipConfigRepository.GetAll().Result.FirstOrDefault()
                : _ipConfigRepository.GetAll().Result.SingleOrDefault(i => i.Id == id);
        }

        public async Task<List<IPConfigEntity>> GetAll()
        {
            return (List<IPConfigEntity>)await _ipConfigRepository.GetAll();
        }

        public async Task Create(IPConfigDTO input)
        {
            await _ipConfigRepository.Add(new IPConfigEntity
            {
                IsActive = true,
                FileName = input.FileName,
                IP = input.Ip,
                IsDeleted = false
            });
        }

        public async Task Delete(int id)
        {
            await _ipConfigRepository.Remove(id);
        }

        public async Task UpdateIpServers()
        {
            var serverActiveIds = await _serverService.GetActiveIds();
            foreach (var activeId in serverActiveIds)
            {
                var server = await _serverService.Detail(activeId);
                var newIp = Get(null);
                if (server is null) continue;
                if (newIp is null) break;
                var config = server.Config;
                var newConfig = _domainService.SetServerAddressStrings(config, newIp.IP);
                server.Config = newConfig.ToString();
                await _serverService.UpdateServer(server);
                await Delete(newIp.Id);
            }
        }
    }
}
