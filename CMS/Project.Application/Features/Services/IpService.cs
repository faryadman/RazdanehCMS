using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions;

        public IpService(IIpRepository jobRepository, IMapper mapper, IMemoryCache memoryCache)
        {
            _ipRepository = jobRepository;
            _mapper = mapper;
            _memoryCache = memoryCache;
            _cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
        }
        public async Task<List<IpDTO>> List()
        {
            if (_memoryCache.TryGetValue("ListIp", out List<IpDTO> cachedIpList))
            {
                var models = _mapper.Map<List<IpDTO>>(cachedIpList.OrderByDescending(i => i.Id).ToList());
                return models;
            }
            else
            {
                var ips = await _ipRepository.GetAll();
                var models = _mapper.Map<List<IpDTO>>(ips.OrderByDescending(i => i.Id).ToList());
                _memoryCache.Set("ListIp", models, _cacheEntryOptions);
                return models;
            }
        }

        public async Task<IpDTO> Detail(string ipName)
        {
            var ip = await _ipRepository.SingleOrDefaultAsync(i => i.Ip == ipName);
            var model = _mapper.Map<IpDTO>(ip);
            return model;
        }

        public async Task Delete(int id)
        {
            await _ipRepository.Remove(id);
        }

        public async Task Delete()
        {
            var ips = await _ipRepository.GetAll();
            foreach (var ip in ips)
            {
                await _ipRepository.RemoveWithoutSaveChange(ip);
            }

            await _ipRepository.SaveChangesTask();
            _memoryCache.Remove("ListIp");
        }
        public async Task Insert(CreateIpDTO input)
        {
            var ipDto = await Detail(input.Ip);
            if (ipDto != null)
            {
                await Update(input);
            }
            else
            {
                await Create(input);
            }
            _memoryCache.Remove("ListIp");
        }
        public async Task Create(CreateIpDTO input)
        {
            var model = _mapper.Map<SaveIP>(input);
            await _ipRepository.Add(model);
        }

        public async Task Update(CreateIpDTO input)
        {
            var model = _mapper.Map<SaveIP>(input);
            model.Tcp = input.Tcp;
            await _ipRepository.Update(model);
        }
    }
}
