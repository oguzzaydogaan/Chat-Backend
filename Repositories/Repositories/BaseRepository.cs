using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repositories.Repositories
{
    public abstract class BaseRepository<TEntity> where TEntity : class
    {
        protected DbContext _context;
        protected DbSet<TEntity> DbSet;
        protected BaseRepository(DbContext context)
        {
            _context = context;
            DbSet = _context.Set<TEntity>();
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await DbSet.FindAsync(id);
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            await DbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            DbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<TEntity> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Entity with id {id} not found.");
            DbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
