using Project.Application.Contracts.Persistence;
using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Persistence.Repositories
{
    public class ApiLogRepository : GenericRepository<ApiLog>, IApiLogRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ApiLogRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
