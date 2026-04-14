using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Shared.Infrastructure.Persistence
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        IQueryable<T> Query(bool tracking = false, bool ignoreQueryFilters = false);
        Task<T?> GetByIdAsync(long id);
        Task<T?> GetByIdForUpdateAsync(long id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddAllAsync(IEnumerable<T> entities);
        Task<T> UpdateAsync(T entity);
        Task<IEnumerable<T>> UpdateAllAsync(IEnumerable<T> entities);
        Task<bool> SoftDeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> expression);
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> filter);
    }
}
