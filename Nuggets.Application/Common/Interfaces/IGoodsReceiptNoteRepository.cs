using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface IGoodsReceiptNoteRepository : IGenericRepository<GoodsReceiptNote>
{
    Task<GoodsReceiptNote?> GetByIdWithLinesAsync(Guid id, CancellationToken ct = default);
}