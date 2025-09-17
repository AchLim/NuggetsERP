using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public sealed record PurchaseReceiptListDto(Guid Id, Guid VendorId, string VendorName, string? ReceiptNumber, DateTime ReceiptDate, PurchaseReceiptStatus Status, PurchaseReceiptPaymentMethod Method, decimal TotalAmount);
public sealed record PurchaseReceiptReadDto(Guid Id, Guid VendorId, string? VendorName, string? ReceiptNumber, DateTime ReceiptDate, PurchaseReceiptStatus Status, PurchaseReceiptPaymentMethod Method, List<PurchaseReceiptLineReadDto> Lines);
public sealed record PurchaseReceiptCreateDto(Guid VendorId, DateTime ReceiptDate, PurchaseReceiptPaymentMethod Method, List<PurchaseReceiptLineCreateDto> Lines);
public sealed record PurchaseReceiptUpdateDto(Guid VendorId, DateTime ReceiptDate, PurchaseReceiptStatus Status, PurchaseReceiptPaymentMethod Method, List<PurchaseReceiptLineUpdateDto> Lines);

public sealed record PurchaseReceiptLineReadDto(Guid Id, Guid ProductId, string? ProductName, Guid UomId, decimal Quantity, decimal UnitCost, decimal LineTotal);
public sealed record PurchaseReceiptLineCreateDto(Guid? Id, Guid ProductId, Guid UomId, decimal Quantity, decimal UnitCost);
public sealed record PurchaseReceiptLineUpdateDto(Guid Id, Guid ProductId, Guid UomId, decimal Quantity, decimal UnitCost);