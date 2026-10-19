using Project.Application.Contracts.Persistence;
using Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Persistence.Repositories
{
    public class ServerLogRepository : GenericRepository<ServerLog>, IServerLogRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ServerLogRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DeleteAllAsync(CancellationToken cancellationToken)
        {
            var watermark = await _dbContext.ServerLogs.MaxAsync(log => (int?)log.Id, cancellationToken);
            if (watermark == null) return;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var batch = _dbContext.ServerLogs.Where(log => log.Id <= watermark).OrderBy(log => log.Id).Take(2000);
                if (await batch.ExecuteDeleteAsync(cancellationToken) == 0) return;
            }
        }
    }
}
