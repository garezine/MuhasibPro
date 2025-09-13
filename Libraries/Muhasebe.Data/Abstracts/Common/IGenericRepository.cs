using Muhasebe.Domain.Common;
using Muhasebe.Domain.Helpers;
using System.Linq.Expressions;

namespace Muhasebe.Data.Abstracts.Common
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        //Common methods
        IQueryable<T> GetQuery(DataRequest<T> request);

        //Sync Methods
        Task<T?> GetByIdAsync(long id);

        Task<IList<T>> GetAllAsync();

        Task<IList<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        Task<bool> AnyAsync(DataRequest<T> request);

        Task<int> CountAsync(DataRequest<T> request);

        // CRUD Operations
        Task AddAsync(T entity);

        Task AddRangeAsync(IList<T> entities);

        Task UpdateAsync(T entity);

        Task UpdateRangeAsync(IList<T> entities);

        Task DeleteAsync(int id);

        Task DeleteAsync(T entity);

        Task DeleteRangeAsync(params T[] entities);

        // Pagination
        Task<IList<T>> GetPagedAsync(
            int skip,
            int take,
            DataRequest<T> request);
    }
}