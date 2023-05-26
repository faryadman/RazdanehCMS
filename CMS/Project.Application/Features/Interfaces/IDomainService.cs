using Project.Application.DTOs.Domain;

namespace Project.Application.Features.Interfaces
{
    public interface IDomainService
    {
        Task<List<DomainDTO>> GetAll();
        Task Create(CreateDomainDTO input);
        Task Delete(int id);
    }
}
