using Project.Application.Contracts.Persistence;
using Project.Domain.Entities;

namespace Project.Persistence.Repositories
{

    public class IpRepository : GenericRepository<SaveIP>, IIpRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public IpRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
