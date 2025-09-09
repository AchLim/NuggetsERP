using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Nuggets.Infrastructure.Persistence;
using System.Linq.Expressions;
using System.Reflection;
using Nuggets.Application.Common.Interfaces;

namespace Nuggets.Infrastructure.Repositories;

public class GenericRepository<T>(NuggetsDbContext db) : IGenericRepository<T>
    where T : class
{
    private readonly DbSet<T> _dbSet = db.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) 
        => await _dbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) 
        => await _dbSet.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) 
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync(ct);

    public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        IDictionary<string, string?>? filters = null,
        string? sort = null,
        CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        // 🔍 Apply column filters
        if (filters != null)
        {
            foreach (var filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Value)) continue;

                var propInfo = typeof(T).GetProperty(
                    filter.Key,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                );

                if (propInfo != null && propInfo.PropertyType == typeof(string))
                {
                    query = query.Where(e =>
                        EF.Functions.ILike(
                            EF.Property<string>(e, propInfo.Name)!,
                            $"%{filter.Value}%"
                        ));
                }
            }
        }

        // Sorting e.g. "Name asc,Email desc"
        if (!string.IsNullOrWhiteSpace(sort))
        {
            // Convert "name:asc,email:desc" => "Name asc, Email desc"
            var normalizedSort = string.Join(",",
                sort.Split(',')
                    .Select(s =>
                    {
                        var parts = s.Split(':');
                        var property = parts[0].Trim();
                        var direction = parts.Length > 1 ? parts[1].Trim() : "asc";

                        // Uppercase property to match C# property names
                        var propInfo = typeof(T).GetProperty(
                            property,
                            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                        );
                        var actualPropName = propInfo?.Name ?? property;

                        return $"{actualPropName} {direction}";
                    })
            );

            query = query.OrderBy(normalizedSort);
        }

        if (string.IsNullOrWhiteSpace(sort))
        {
            var keyProp = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (keyProp != null)
            {
                query = query.OrderBy(keyProp.Name);
            }
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

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
        _dbSet.Update(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}
