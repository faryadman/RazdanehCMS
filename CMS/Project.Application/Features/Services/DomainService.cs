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
        private readonly IMapper _mapper;

        public DomainService(IMapper mapper, IDomainRepository domainRepository)
        {
            _mapper = mapper;
            _domainRepository = domainRepository;
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
            query = filter == 1 ? query.Where(x => x.IsActive) : query.Where(x => x.IsActive == false);
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
        public Task Remove()
        {
            var query = _domainRepository.GetAllQueryable();
            query = query.Where(x => !x.IsActive);
            var list = query.ToList();
            foreach (var domain in list)
            {
                _domainRepository.Remove(domain);
            }

            _domainRepository.SaveChangesTask();
            return Task.CompletedTask;
        }

        public async Task<List<Domain.Entities.Domain>> ListInactiveDomain()
        {
            var listInactive = await GetByFilter(0);
            var list = _mapper.Map<IEnumerable<DomainDTO>, List<Domain.Entities.Domain>>(listInactive);
            return list;
        }
    }
}
