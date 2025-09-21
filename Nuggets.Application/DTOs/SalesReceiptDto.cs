using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public sealed record SalesReceiptListDto(
    Guid Id,
    Guid CustomerId,
    string? CustomerName,
    string? ReceiptNumber,
    DateTime ReceiptDate,
    SalesReceiptStatus Status,
    SalesReceiptPaymentMethod Method,
    decimal TotalAmount
);

public sealed record SalesReceiptLineReadDto(
    Guid Id,
    Guid ProductId,
    string? ProductName,
    Guid UomId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal LineTotal
);

public sealed record SalesReceiptReadDto(
    Guid Id,
    Guid CustomerId,
    string? CustomerName,
    string? ReceiptNumber,
    DateTime ReceiptDate,
    SalesReceiptStatus Status,
    SalesReceiptPaymentMethod Method,
    List<SalesReceiptLineReadDto> Lines
);

public sealed record SalesReceiptLineCreateDto(Guid? Id, Guid ProductId, Guid UomId, decimal Quantity, decimal UnitPrice, decimal DiscountPercent);
public sealed record SalesReceiptCreateDto(Guid CustomerId, DateTime ReceiptDate, SalesReceiptPaymentMethod Method, List<SalesReceiptLineCreateDto> Lines);

public sealed record SalesReceiptLineUpdateDto(Guid Id, Guid ProductId, Guid UomId, decimal Quantity, decimal UnitPrice, decimal DiscountPercent);
public sealed record SalesReceiptUpdateDto(Guid CustomerId, DateTime ReceiptDate, SalesReceiptStatus Status, SalesReceiptPaymentMethod Method, List<SalesReceiptLineUpdateDto> Lines);