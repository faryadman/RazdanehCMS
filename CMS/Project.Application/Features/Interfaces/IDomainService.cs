using Project.Application.DTOs.Domain;

namespace Project.Application.Features.Interfaces
{
    public interface IDomainService
    {
        Task<List<DomainDTO>> GetAll();
        Task<List<DomainDTO>> GetByFilter(int filter);
        Task Create(CreateDomainDTO input);
        Task Delete(int id);
        Task<List<Domain.Entities.Domain>> ListInactiveDomain();
    }
}