using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public sealed record VendorBillListDto(Guid Id, string? BillNumber, Guid VendorId, string? VendorName, DateTime BillDate, Guid? PurchaseOrderId, string? OrderNumber, string Status, decimal TotalAmount);
public sealed record VendorBillReadDto(
    Guid Id, 
    string? BillNumber,
    Guid VendorId,
    string? VendorName,
    DateTime BillDate,
    Guid? PurchaseOrderId,
    string? OrderNumber,
    VendorBillStatus Status,
    List<VendorBillLineReadDto> Lines,
    decimal PaidAmount,
    decimal BilledAmount
    
);
public sealed record VendorBillUpdateDto(Guid VendorId, Guid? PurchaseOrderId, DateTime BillDate, VendorBillStatus Status, List<VendorBillLineUpdateDto> Lines);
public sealed record VendorBillCreateDto(Guid VendorId, Guid? PurchaseOrderId, DateTime BillDate, List<VendorBillLineCreateDto> Lines);

public sealed record VendorBillLineReadDto(Guid Id, Guid ProductId, string? ProductName, Guid UomId, decimal Quantity, decimal UnitCost, decimal DiscountPercent, decimal LineTotal);
public sealed record VendorBillLineCreateDto(Guid? Id, Guid ProductId, Guid UomId, decimal Quantity, decimal UnitCost, decimal DiscountPercent);
public sealed record VendorBillLineUpdateDto(Guid Id, Guid ProductId, Guid UomId, decimal Quantity, decimal UnitCost, decimal DiscountPercent);