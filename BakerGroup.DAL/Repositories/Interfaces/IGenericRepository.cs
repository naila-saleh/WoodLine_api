using System.Linq.Expressions;

namespace BakerGroup.DAL.Repositories.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
    Task<(IEnumerable<T> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? filter = null, string? includeProperties = null, Expression<Func<T, object>>? orderBy = null);
    Task<T?> GetByIdAsync(string id, string? includeProperties = null);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> SaveAsync();
}
