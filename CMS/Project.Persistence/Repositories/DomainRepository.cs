#nullable enable
using Microsoft.EntityFrameworkCore;
using Project.Application.Contracts.Persistence;
using DomainEntity = Project.Domain.Entities.Domain;

namespace Project.Persistence.Repositories
{
    public class DomainRepository : GenericRepository<DomainEntity>, IDomainRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DomainRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        private IQueryable<DomainEntity> ActiveDomains => _dbContext.Domains.AsNoTracking()
            .Where(domain => domain.IsActive && !domain.IsDeleted);

        public async Task<IReadOnlyList<DomainEntity>> ListAsync(bool active, CancellationToken cancellationToken) =>
            await _dbContext.Domains.AsNoTracking().Where(domain => domain.IsActive == active && !domain.IsDeleted)
                .OrderBy(domain => domain.Id).ToListAsync(cancellationToken);

        public Task<DomainEntity?> GetNextActiveAsync(CancellationToken cancellationToken) => ActiveDomains
            .Where(domain => domain.DomainName != null && domain.DomainName.Trim() != "")
            .OrderBy(domain => domain.ZoneId == null || domain.ZoneId.Trim() == "")
            .ThenBy(domain => domain.Id).FirstOrDefaultAsync(cancellationToken);

        // A consumed pool item is inactive, but still identifies the base of an already assigned server.
        // Rotation of a new base always uses GetNextActiveAsync instead.
        public Task<DomainEntity?> GetRegisteredByHostAsync(string host, CancellationToken cancellationToken) =>
            _dbContext.Domains.AsNoTracking().Where(domain => !domain.IsDeleted && domain.DomainName != null && domain.DomainName != ""
                && (domain.DomainName == host || host.EndsWith("." + domain.DomainName)))
                .OrderByDescending(domain => domain.DomainName.Length).ThenBy(domain => domain.Id).FirstOrDefaultAsync(cancellationToken);

        public Task<DomainEntity?> GetActiveByIdAsync(int id, CancellationToken cancellationToken) => ActiveDomains
            .SingleOrDefaultAsync(domain => domain.Id == id, cancellationToken);

        public async Task<IReadOnlyList<DomainEntity>> ListMissingZoneAsync(CancellationToken cancellationToken) =>
            await ActiveDomains.Where(domain => (domain.ZoneId == null || domain.ZoneId.Trim() == "")
                && domain.DomainName != null && domain.DomainName.Trim() != "")
                .OrderBy(domain => domain.Id).ToListAsync(cancellationToken);

        public async Task<bool> TrySetZoneAsync(int id, string expectedName, string zoneId, CancellationToken cancellationToken) =>
            await _dbContext.Domains.Where(domain => domain.Id == id && domain.IsActive && !domain.IsDeleted
                && domain.DomainName == expectedName
                && (domain.ZoneId == null || domain.ZoneId.Trim() == ""))
                .ExecuteUpdateAsync(update => update.SetProperty(domain => domain.ZoneId, zoneId)
                    .SetProperty(domain => domain.UpdatedAt, DateTime.Now)
                    .SetProperty(domain => domain.UpdatedBy, "SYSTEM"), cancellationToken) > 0;

        public async Task SetInactiveAsync(int id, CancellationToken cancellationToken) =>
            await _dbContext.Domains.Where(domain => domain.Id == id && domain.IsActive)
                .ExecuteUpdateAsync(update => update.SetProperty(domain => domain.IsActive, false)
                    .SetProperty(domain => domain.UpdatedAt, DateTime.Now)
                    .SetProperty(domain => domain.UpdatedBy, "SYSTEM"), cancellationToken);

        public Task<int> DeleteInactiveAsync(CancellationToken cancellationToken) =>
            _dbContext.Domains.Where(domain => !domain.IsActive).ExecuteDeleteAsync(cancellationToken);
    }
}
