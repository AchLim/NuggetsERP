using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public sealed record SalesOrderListDto(
    Guid Id,
    Guid CustomerId,
    string? CustomerName,
    string? OrderNumber,
    DateTime OrderDate,
    SalesOrderStatus Status,
    decimal TotalAmount
);

public sealed record SalesOrderLineReadDto(
    Guid Id,
    Guid ProductId,
    string? ProductName,
    Guid UomId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal LineTotal
);

public sealed record SalesOrderReadDto(
    Guid Id,
    Guid CustomerId,
    string? CustomerName,
    string OrderNumber,
    DateTime OrderDate,
    SalesOrderStatus Status,
    IReadOnlyList<SalesOrderLineReadDto> Lines,
    decimal OrderedQty,
    decimal DeliveredQty,
    decimal InvoicedQty
);

public sealed record SalesOrderLineCreateDto(
    Guid? Id,
    Guid ProductId,
    Guid UomId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercent
);

public sealed record SalesOrderCreateDto(
    Guid CustomerId,
    DateTime OrderDate,
    List<SalesOrderLineCreateDto> Lines
);

public sealed record SalesOrderLineUpdateDto(
    Guid? Id,
    Guid ProductId,
    Guid UomId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercent
);

public sealed record SalesOrderUpdateDto(
    Guid CustomerId,
    DateTime OrderDate,
    SalesOrderStatus Status,
    List<SalesOrderLineUpdateDto> Lines
);