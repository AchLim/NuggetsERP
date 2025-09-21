using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public sealed record PurchaseOrderListDto(Guid Id, Guid VendorId, string? VendorName, string? OrderNumber, DateTime OrderDate, string Status, decimal TotalAmount);

public record PurchaseOrderReadDto(
    Guid Id,
    Guid VendorId,
    string? VendorName,
    string OrderNumber,
    DateTime OrderDate,
    PurchaseOrderStatus Status,
    IReadOnlyList<PurchaseOrderLineReadDto> Lines,
    decimal OrderedQty,
    decimal ReceivedQty,
    decimal BilledQty
);

public sealed record PurchaseOrderCreateDto(Guid VendorId, DateTime OrderDate, List<PurchaseOrderLineCreateDto> Lines);
public sealed record PurchaseOrderUpdateDto(Guid VendorId, DateTime OrderDate, PurchaseOrderStatus Status, List<PurchaseOrderLineUpdateDto> Lines);

public sealed record PurchaseOrderLineReadDto(
    Guid Id,
    Guid ProductId,
    string? ProductName,
    Guid UomId,
    decimal Quantity,
    decimal UnitCost,
    decimal DiscountPercent,
    decimal LineTotal,
    decimal RemainingQty
);

public sealed record PurchaseOrderLineCreateDto(Guid? Id, Guid ProductId, Guid UomId, decimal Quantity, decimal UnitCost, decimal DiscountPercent);
public sealed record PurchaseOrderLineUpdateDto(Guid Id, Guid ProductId, Guid UomId, decimal Quantity, decimal UnitCost, decimal DiscountPercent);