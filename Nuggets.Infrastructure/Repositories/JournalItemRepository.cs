using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public sealed class JournalItemRepository(NuggetsDbContext db)
    : GenericRepository<JournalItem>(db), IJournalItemRepository
{
}