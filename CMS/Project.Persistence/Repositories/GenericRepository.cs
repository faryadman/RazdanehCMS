using Microsoft.EntityFrameworkCore;
using Project.Application.Contracts.Persistence;
using Project.Domain.Entities.Base;
using System.Linq.Expressions;

namespace Project.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private async Task<T> Get(int id)
        {
            return await _dbContext.Set<T>().FirstOrDefaultAsync(f => (f as BaseEntity).IsActive == true && (f as BaseEntity).Id == id);
        }

        public async Task<T> GetNoTracking(int id)
        {
            return await _dbContext.Set<T>().AsNoTracking().FirstOrDefaultAsync(f => (f as BaseEntity).IsActive == true && (f as BaseEntity).Id == id);
        }

        public async Task<IReadOnlyList<T>> GetAll()
        {
            return await _dbContext.Set<T>().Where(w => (w as BaseEntity).IsActive == true).ToListAsync();
        }

        public IQueryable<T> GetAllQueryable()
        {
            return _dbContext.Set<T>().AsQueryable();
        }

        public async Task<T> Add(T entity)
        {
            await _dbContext.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task Update(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var find = await Get(id);
            (find as BaseEntity).IsActive = false;
            //_dbContext.Set<T>().Update(find);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Recover(int id)
        {
            var find = await Get(id);
            (find as BaseEntity).IsActive = true;
            //_dbContext.Set<T>().Update(find);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> Exist(int id)
        {
            var entity = await GetNoTracking(id);
            return entity != null;
        }
        public async Task<bool> Exist(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().Where(predicate).AnyAsync();
        }
        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _dbContext.Set<T>()
                .Where(w => (w as BaseEntity).IsActive == true)
                .Where(predicate);
        }

        public IQueryable<T> FindQueryable(Expression<Func<T, bool>> predicate)
        {
            return _dbContext.Set<T>()
                .Where(w => (w as BaseEntity).IsActive == true)
                .Where(predicate).AsQueryable();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>()
                .Where(w => (w as BaseEntity).IsActive == true)
                .Where(predicate).ToListAsync();
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>()
                .Where(w => (w as BaseEntity).IsActive == true)
                .SingleOrDefaultAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>()
                .Where(w => (w as BaseEntity).IsActive == true)
                .Where(predicate).CountAsync();
        }

        public async Task Remove(int id)
        {
            var find = await Get(id);
            if (find != null)
            {

                _dbContext.Remove(find);
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task Remove(T ob)
        {
            if (ob != null)
            {
                _dbContext.Remove(ob);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}