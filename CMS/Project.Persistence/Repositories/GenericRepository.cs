using Microsoft.EntityFrameworkCore;
using Project.Application.Contracts.Persistence;
using Project.Domain.Entities.Base;
using System.Linq.Expressions;

namespace Project.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly ApplicationDbContext _dbContext;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private IQueryable<T> ActiveEntities => _dbContext.Set<T>().AsQueryable();

        public async Task<T> Get(int id)
        {
            return await ActiveEntities.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<T> GetNoTracking(int id)
        {
            return await ActiveEntities.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IReadOnlyList<T>> GetAll()
        {
            return await ActiveEntities.AsNoTracking().ToListAsync();
        }

        public IQueryable<T> GetAllQueryable()
        {
            return ActiveEntities.AsNoTracking();
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
            var entity = await Get(id);
            if (entity != null)
            {
                entity.IsActive = false;
                _dbContext.Entry(entity).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task Recover(int id)
        {
            var entity = await Get(id);
            if (entity != null)
            {
                entity.IsActive = true;
                _dbContext.Entry(entity).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> Exist(int id)
        {
            return await ActiveEntities.AnyAsync(e => e.Id == id);
        }

        public async Task<bool> Exist(Expression<Func<T, bool>> predicate)
        {
            return await ActiveEntities.AnyAsync(predicate);
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return ActiveEntities.Where(predicate).AsNoTracking();
        }

        public IQueryable<T> FindQueryable(Expression<Func<T, bool>> predicate)
        {
            return ActiveEntities.Where(predicate).AsNoTracking();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await ActiveEntities.Where(predicate).AsNoTracking().ToListAsync();
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await ActiveEntities.AsNoTracking().SingleOrDefaultAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await ActiveEntities.CountAsync(predicate);
        }

        public async Task Remove(int id)
        {
            var entity = await Get(id);
            if (entity != null)
            {
                _dbContext.Set<T>().Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public Task RemoveWithoutSaveChange(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }

        public Task SaveChangesTask()
        {
            _dbContext.SaveChanges();
            return Task.CompletedTask;
        }


    }

}