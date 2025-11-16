using Microsoft.EntityFrameworkCore;
using Muhasib.Data.BaseRepositories.Contracts;
using Muhasib.Data.Common;
using Muhasib.Domain.Entities;
using System.Linq.Expressions;

namespace Muhasib.Data.BaseRepositories
{
    /// <summary>
    /// Tüm repository'lerin türeyeceği base class
    /// Concrete DbContext tipini alır, böylece DI sorunsuz çalışır
    /// </summary>
    public abstract class BaseRepository<TContext, T> : IRepository<T>
        where TContext : DbContext
        where T : BaseEntity, new()
    {
        protected readonly TContext Context;
        protected readonly DbSet<T> DbSet;

        protected BaseRepository(TContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            DbSet = Context.Set<T>();
        }

        public virtual IQueryable<T> GetQuery(DataRequest<T> request)
        {
            IQueryable<T> items = DbSet;

            // Query (Arama)
            if (!string.IsNullOrEmpty(request.Query))
            {
                items = items.Where(r => r.ArananTerim.Contains(request.Query.ToLower()));
            }

            // Where koşulu
            if (request.Where != null)
            {
                items = items.Where(request.Where);
            }

            // Sıralama
            if (request.OrderBy != null)
            {
                items = items.OrderBy(request.OrderBy);
            }
            if (request.OrderByDesc != null)
            {
                items = items.OrderByDescending(request.OrderByDesc);
            }

            // Include'lar
            if (request.Includes != null)
            {
                foreach (var include in request.Includes)
                    items = items.Include(include);
            }

            return items;
        }

        #region CRUD Operations

        public virtual async Task AddAsync(T entity)
        {
            await DbSet.AddAsync(entity).ConfigureAwait(false);
        }

        public virtual async Task AddRangeAsync(IList<T> entities)
        {
            await DbSet.AddRangeAsync(entities).ConfigureAwait(false);
        }

        public virtual Task UpdateAsync(T entity)
        {
            DbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task UpdateRangeAsync(IList<T> entities)
        {
            DbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(long id)
        {
            var entity = await GetByIdAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                DbSet.Remove(entity);
            }
        }

        public virtual Task DeleteAsync(T entity)
        {
            DbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteRangeAsync(params T[] entities)
        {
            DbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        #endregion

        #region Query Operations

        public virtual async Task<T?> GetByIdAsync(long id)
        {
            return await DbSet.FindAsync(id).ConfigureAwait(false);
        }

        public virtual async Task<IList<T>> GetAllAsync()
        {
            return await DbSet.AsNoTracking().ToListAsync().ConfigureAwait(false);
        }

        public virtual async Task<T> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.Where(predicate).AsNoTracking().FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.FirstOrDefaultAsync(predicate).ConfigureAwait(false);
        }

        public virtual async Task<IList<T>> GetPagedAsync(int skip, int take, DataRequest<T> request)
        {
            IQueryable<T> query = GetQuery(request);
            return await query.Skip(skip).Take(take).AsNoTracking().ToListAsync().ConfigureAwait(false);
        }

        #endregion

        #region Aggregate Operations

        public virtual async Task<bool> AnyAsync(DataRequest<T> request)
        {
            IQueryable<T> items = DbSet;

            if (!string.IsNullOrEmpty(request.Query))
            {
                items = items.Where(r => r.ArananTerim.Contains(request.Query.ToLower()));
            }

            if (request.Where != null)
            {
                items = items.Where(request.Where);
            }

            return await items.AnyAsync().ConfigureAwait(false);
        }

        public virtual async Task<int> CountAsync(DataRequest<T> request)
        {
            IQueryable<T> items = DbSet;

            if (!string.IsNullOrEmpty(request.Query))
            {
                items = items.Where(r => r.ArananTerim.Contains(request.Query.ToLower()));
            }

            if (request.Where != null)
            {
                items = items.Where(request.Where);
            }

            return await items.CountAsync().ConfigureAwait(false);
        }

        #endregion
    }
}