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

        public IPConfigEntity Get(string ip)
        {
            return ip is null
                ? null
                : _ipConfigRepository.GetAll().Result.SingleOrDefault(i => i.IP == ip);
        }
        public async Task<List<IPConfigEntity>> GetAll()
        {
            return (List<IPConfigEntity>)await _ipConfigRepository.GetAll();
        }

        public IEnumerable<IPConfigEntity> ListInactive()
        {
            return _ipConfigRepository.GetAll().Result.Where(i => i.IsActive == false && i.IsDeleted == true).ToList();
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
        public async Task Inactive(int id)
        {
            var entity = _ipConfigRepository.Find(i => i.Id == id).FirstOrDefault();
            entity.IsDeleted = true;
            entity.IsActive = false;
            await _ipConfigRepository.Update(entity);
        }
        public async Task GenerateUpdateAsync(string serverId, string email, string apiKey)
        {
            await UpdateIpServers(serverId);
        }

        public async Task UpdateIpServers(string serverId)
        {
            var server = await _serverService.Detail(serverId);
            if (server is null) return;
            var newIp = Get((int?)null);
            if (newIp is null) return;

            var config = server.Config;
            var newConfig = _domainService.SetServerAddressStrings(config, newIp.IP);
            server.Config = newConfig.ToString();
            await _serverService.UpdateServer(server);

            var ip = _domainService.GetServerAddressStrings(config);
            var oldIP = Get(ip);
            if (oldIP != null)
            {
                await Inactive(oldIP.Id);
            }

        }
    }
}
