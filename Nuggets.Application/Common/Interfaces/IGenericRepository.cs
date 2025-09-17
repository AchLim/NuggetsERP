using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Nuggets.Application.Common.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    
    Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize,
        IQueryable<T>? startingQuery = null,
        CancellationToken ct = default
    );

    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task<T> UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);

    Task<IDbContextTransaction> BeginTransactionAsync();

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<long> GetNextSequenceValueAsync(string sequenceName, CancellationToken ct = default);
}