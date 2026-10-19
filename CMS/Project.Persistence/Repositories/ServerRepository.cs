using Microsoft.EntityFrameworkCore;
using Project.Application.Contracts.Persistence;
using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Persistence.Repositories
{
    public class ServerRepository : GenericRepository<Server>, IServerRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ServerRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Server> GetActiveWithGroupAsync(int id)
        {
            return _dbContext.Servers.AsNoTracking()
                .Include(server => server.Group)
                .SingleOrDefaultAsync(server => server.Id == id && server.IsActive);
        }
    }
}
