using Project.Application.Contracts.Persistence;

namespace Project.Persistence.Repositories
{
    public class DomainRepository : GenericRepository<Domain.Entities.Domain>, IDomainRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DomainRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
