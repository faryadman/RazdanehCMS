#nullable enable
using DomainEntity = Project.Domain.Entities.Domain;

namespace Project.Application.Contracts.Persistence
{
    public interface IDomainRepository : IGenericRepository<DomainEntity>
    {
        Task<IReadOnlyList<DomainEntity>> ListAsync(bool active, CancellationToken cancellationToken);
        Task<DomainEntity?> GetNextActiveAsync(CancellationToken cancellationToken);
        Task<DomainEntity?> GetRegisteredByHostAsync(string host, CancellationToken cancellationToken);
        Task<DomainEntity?> GetActiveByIdAsync(int id, CancellationToken cancellationToken);
        Task<IReadOnlyList<DomainEntity>> ListMissingZoneAsync(CancellationToken cancellationToken);
        Task<bool> TrySetZoneAsync(int id, string expectedName, string zoneId, CancellationToken cancellationToken);
        Task SetInactiveAsync(int id, CancellationToken cancellationToken);
        Task<int> DeleteInactiveAsync(CancellationToken cancellationToken);
    }
}
