using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public sealed record VendorBillListDto(Guid Id, Guid VendorId, string? VendorName, DateTime BillDate, string Status, decimal TotalAmount);
public sealed record VendorBillLineReadDto(Guid Id, Guid ProductId, string? ProductName, Guid UomId, decimal Quantity, decimal UnitCost, decimal LineTotal);
public sealed record VendorBillReadDto(Guid Id, Guid VendorId, string? VendorName, DateTime BillDate, VendorBillStatus Status, List<VendorBillLineReadDto> Lines);
public sealed record VendorBillLineCreateDto(Guid? Id, Guid ProductId, Guid UomId, decimal Quantity, decimal UnitCost);
public sealed record VendorBillCreateDto(Guid VendorId, DateTime BillDate, List<VendorBillLineCreateDto> Lines);
public sealed record VendorBillLineUpdateDto(Guid Id, Guid ProductId, Guid UomId, decimal Quantity, decimal UnitCost);
public sealed record VendorBillUpdateDto(Guid VendorId, DateTime BillDate, VendorBillStatus Status, List<VendorBillLineUpdateDto> Lines);