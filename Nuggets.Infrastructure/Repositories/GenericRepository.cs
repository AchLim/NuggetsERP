using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public class GenericRepository<T>(NuggetsDbContext db) : IGenericRepository<T>
    where T : class
{
    private readonly DbSet<T> _dbSet = db.Set<T>();
    
    public IQueryable<T> Query() => _dbSet.AsNoTracking();
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.FindAsync([id], ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking().ToListAsync(ct);

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync(ct);

    public virtual async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        IQueryable<T>? startingQuery = null,
        CancellationToken ct = default)
    {
        var query = startingQuery ?? _dbSet.AsNoTracking().AsQueryable();

        // Default to Id order if exists
        var keyProp = typeof(T).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (keyProp != null)
        {
            query = query.OrderBy(e => EF.Property<object>(e, keyProp.Name));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken ct = default)
    {
        // _dbSet.Update(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public virtual async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await db.Database.BeginTransactionAsync();
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await db.SaveChangesAsync(ct);
    }
    
    
    public virtual async Task<long> GetNextSequenceValueAsync(string sequenceName, IDbContextTransaction transaction, CancellationToken ct = default)
    {
        var dbTransaction = transaction.GetDbTransaction();
        var connection = dbTransaction.Connection ?? db.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.Transaction = dbTransaction;
        command.CommandText = $"SELECT nextval('{sequenceName}')";

        var result = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt64(result);
    }
}