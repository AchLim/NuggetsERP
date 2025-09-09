using System.Linq.Expressions;

namespace Nuggets.Application.Common.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    
    Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize,
        IDictionary<string, string?>? filters = null,
        string? sort = null,
        CancellationToken ct = default
    );

    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task<T> UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}