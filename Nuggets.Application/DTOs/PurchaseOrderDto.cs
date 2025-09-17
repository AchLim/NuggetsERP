using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public sealed record PurchaseOrderListDto(Guid Id, Guid VendorId, string? VendorName, string? OrderNumber, DateTime OrderDate, string Status, decimal TotalAmount);
public sealed record PurchaseOrderReadDto(Guid Id, Guid VendorId, string? VendorName, string? OrderNumber, DateTime OrderDate, PurchaseOrderStatus Status, List<PurchaseOrderLineReadDto> Lines);
public sealed record PurchaseOrderCreateDto(Guid VendorId, DateTime OrderDate, List<PurchaseOrderLineCreateDto> Lines);
public sealed record PurchaseOrderUpdateDto(Guid VendorId, DateTime OrderDate, PurchaseOrderStatus Status, List<PurchaseOrderLineUpdateDto> Lines);

public sealed record PurchaseOrderLineReadDto(Guid Id, Guid ProductId, string? ProductName, Guid UomId, decimal Quantity, decimal UnitCost, decimal LineTotal);
public sealed record PurchaseOrderLineCreateDto(Guid? Id, Guid ProductId, Guid UomId, decimal Quantity, decimal UnitCost);
public sealed record PurchaseOrderLineUpdateDto(Guid Id, Guid ProductId, Guid UomId, decimal Quantity, decimal UnitCost);