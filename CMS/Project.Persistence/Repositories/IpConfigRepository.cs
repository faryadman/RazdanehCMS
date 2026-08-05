using Project.Application.Contracts.Persistence;
using Project.Domain.Entities;

namespace Project.Persistence.Repositories
{
    public class IpConfigRepository : GenericRepository<IPConfigEntity>, IIpConfigRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public IpConfigRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
