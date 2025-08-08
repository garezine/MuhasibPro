using Microsoft.EntityFrameworkCore;
using Muhasebe.Domain.Common;
using Muhasebe.Domain.Interfaces.Database;
using System.Linq.Expressions;

namespace Muhasebe.Data.EfRepositories.Common
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity, new()
    {
        protected readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IQueryable<T> GetQuery(DataRequest<T> request)
        {
            IQueryable<T> items = _dbSet;
            if(!string.IsNullOrEmpty(request.Query))
            {
                items = items.Where(r => r.SearchTerms.Contains(request.Query));
            }
            // Where
            if(request.Where != null)
            {
                items = items.Where(request.Where);
            }

            // Order By
            if(request.OrderBy != null)
            {
                items = items.OrderBy(request.OrderBy);
            }
            if(request.OrderByDesc != null)
            {
                items = items.OrderByDescending(request.OrderByDesc);
            }
            if(request.Includes != null)
            {
                foreach(var include in request.Includes)
                    items = items.Include(include);
            }

            return items;
        }

        #region CRUD
        public virtual async Task AddAsync(T entity) { await _dbSet.AddAsync(entity).ConfigureAwait(false); }

        public virtual async Task AddRangeAsync(IList<T> entities)
        {
            var entityList = entities.ToList();
            await _dbSet.AddRangeAsync(entityList).ConfigureAwait(false);
        }

        public virtual Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task UpdateRangeAsync(IList<T> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if(entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public virtual Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteRangeAsync(params T[] entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public async Task<IList<T>> GetPagedAsync(int skip, int take, DataRequest<T> request)
        {
            IQueryable<T> query = GetQuery(request);
            var records = await query.Skip(skip).Take(take).AsNoTracking().ToListAsync();
            return records;
        }
        #endregion

        #region Sorgu Metodları
        public virtual async Task<T?> GetByIdAsync(long id) { return await _dbSet.FindAsync(id).ConfigureAwait(false); }
        public virtual async Task<IList<T>> GetAllAsync() { return await _dbSet.ToListAsync().ConfigureAwait(false); }
        public virtual async Task<IList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        { return await _dbSet.Where(predicate).ToListAsync().ConfigureAwait(false); }
        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        { return await _dbSet.FirstOrDefaultAsync(predicate).ConfigureAwait(false); }

        public virtual async Task<bool> AnyAsync(DataRequest<T> request)
        {
            IQueryable<T> items = _dbSet;
            // Query
            if(!String.IsNullOrEmpty(request.Query))
            {
                items = items.Where(r => r.SearchTerms.Contains(request.Query.ToLower()));
            }

            // Where
            if(request.Where != null)
            {
                items = items.Where(request.Where);
            }

            return await items.AnyAsync();
        }

        public virtual async Task<int> CountAsync(DataRequest<T> request)
        {
            IQueryable<T> items = _dbSet;
            // Query
            if(!String.IsNullOrEmpty(request.Query))
            {
                items = items.Where(r => r.SearchTerms.Contains(request.Query.ToLower()));
            }

            // Where
            if(request.Where != null)
            {
                items = items.Where(request.Where);
            }

            return await items.CountAsync().ConfigureAwait(false);
        }
    }
    #endregion
}
