using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public record StockMovementDto(
    Guid Id,
    string ProductName,
    StockMovementType MovementType,
    decimal Quantity,
    decimal UnitCost,
    DateTime MovementDate,
    string? ReferenceType,
    Guid? ReferenceId,
    string? ReferenceUrl,
    string? Status
);

public record ProductInventoryDto(
    string ProductName,
    decimal CurrentStock,
    decimal CurrentMovingAverageCost,
    decimal CurrentInventoryValue,
    IReadOnlyList<StockMovementDto> Movements
);

public class InventoryAdjustmentDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public DateTime MovementDate { get; set; }
    public decimal StockBefore { get; set; }
    public decimal StockAfter { get; set; }
    public decimal AvgCostBefore { get; set; }
    public decimal AvgCostAfter { get; set; }
    public decimal InventoryValueBefore { get; set; }
    public decimal InventoryValueAfter { get; set; }
    public string Status { get; set; } = string.Empty;
}