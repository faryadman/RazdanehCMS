using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Project.Application.Contracts.Persistence;
using Project.Application.DTOs.Domain;
using Project.Application.Features.Interfaces;

namespace Project.Application.Features.Services
{
    public class DomainService : IDomainService
    {
        private readonly IDomainRepository _domainRepository;
        private readonly IAppSettingService _appSettingService;
        private readonly IApiLogService _apiLogService;
        private readonly IMapper _mapper;
        private readonly IOperatorIdentificationService _operatorIdentificationService;

        public DomainService(IDomainRepository domainRepository, IMapper mapper, IAppSettingService appSettingService, IApiLogService apiLogService, IOperatorIdentificationService operatorIdentificationService)
        {
            _domainRepository = domainRepository;
            _mapper = mapper;
            _appSettingService = appSettingService;
            _apiLogService = apiLogService;
            _operatorIdentificationService = operatorIdentificationService;
        }
        public async Task<List<DomainDTO>> GetAll()
        {
            var list = await _domainRepository.GetAll();
            var model = _mapper.Map<IEnumerable<Domain.Entities.Domain>, List<DomainDTO>>(list.Where(x => !x.IsDeleted));
            return model;
        }

        public async Task<List<DomainDTO>> GetByFilter(int filter)
        {
            var query = _domainRepository.GetAllQueryable();
            query = filter == 1 ? query.Where(x => x.IsActive) : query.Where(x => !x.IsActive);
            var data = await query.ToListAsync();
            var list = _mapper.Map<IEnumerable<Domain.Entities.Domain>, List<DomainDTO>>(data);
            return list;
        }

        public async Task Create(CreateDomainDTO input)
        {
            var model = _mapper.Map<Domain.Entities.Domain>(input);
            await _domainRepository.Add(model);
        }

        public async Task Delete(int id)
        {
            await _domainRepository.Delete(id);
        }

        public async Task DeleteInactiveDomain()
        {
            var listInactive = await GetByFilter(0);
            var list = _mapper.Map<IEnumerable<DomainDTO>, List<Domain.Entities.Domain>>(listInactive);
            foreach (var domain in list)
            {
                await _domainRepository.Remove(domain);
            }
        }
    }
}
