using Project.Application.Contracts.Persistence;
using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Persistence.Repositories
{
    public class OperatorIdentificationRepository : GenericRepository<OperatorIdentification>, IOperatorIdentificationRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public OperatorIdentificationRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
