using Muhasib.Data.Common;
using Muhasib.Domain.Entities;
using System.Linq.Expressions;

namespace Muhasib.Data.BaseRepositories.Contracts
{
    /// <summary>
    /// Tüm repository'lerin implement edeceği base interface
    /// Generic CRUD ve sorgu operasyonlarını içerir
    /// </summary>
    public interface IRepository<T> where T : BaseEntity, new()
    {
        // CRUD Operations
        Task AddAsync(T entity);
        Task AddRangeAsync(IList<T> entities);
        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(IList<T> entities);
        Task DeleteAsync(long id);
        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(params T[] entities);

        // Query Operations
        Task<T?> GetByIdAsync(long id);
        Task<IList<T>> GetAllAsync();
        Task<T> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        // Paged & Advanced Queries
        Task<IList<T>> GetPagedAsync(int skip, int take, DataRequest<T> request);
        IQueryable<T> GetQuery(DataRequest<T> request);

        // Aggregate Operations
        Task<bool> AnyAsync(DataRequest<T> request);
        Task<int> CountAsync(DataRequest<T> request);
    }
}