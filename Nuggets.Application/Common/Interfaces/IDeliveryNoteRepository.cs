using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface IDeliveryNoteRepository : IGenericRepository<DeliveryNote>
{
    Task<DeliveryNote?> GetByIdWithLinesAsync(Guid id, CancellationToken ct = default);
}