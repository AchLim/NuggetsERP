using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public record StockMovementDto(
    Guid Id,
    string ProductName,
    StockMovementType MovementType,
    decimal Quantity,
    DateTime MovementDate,
    string? ReferenceType,
    Guid? ReferenceId,
    string? ReferenceUrl
);

public record ProductInventoryDto(
    string ProductName,
    decimal CurrentStock,
    IReadOnlyList<StockMovementDto> Movements
);