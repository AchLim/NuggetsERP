using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public class CompanyRepository(NuggetsDbContext db) : GenericRepository<Company>(db), ICompanyRepository
{
    private readonly NuggetsDbContext _db = db;

    public async Task<IReadOnlyList<Company>> GetUserCompaniesAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.UserCompanies
            .Where(uc => uc.UserId == userId)
            .Select(uc => uc.Company)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> GetUserCompanyIdsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.UserCompanies
            .Where(uc => uc.UserId == userId)
            .Select(uc => uc.CompanyId)
            .ToListAsync(ct);
    }
}