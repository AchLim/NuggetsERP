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
    decimal LineTotal
);

public sealed record SalesOrderReadDto(
    Guid Id,
    Guid CustomerId,
    string? CustomerName,
    string? OrderNumber,
    DateTime OrderDate,
    SalesOrderStatus Status,
    List<SalesOrderLineReadDto> Lines
);

public sealed record SalesOrderLineCreateDto(
    Guid? Id,
    Guid ProductId,
    Guid UomId,
    decimal Quantity,
    decimal UnitPrice
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
    decimal UnitPrice
);

public sealed record SalesOrderUpdateDto(
    Guid CustomerId,
    DateTime OrderDate,
    SalesOrderStatus Status,
    List<SalesOrderLineUpdateDto> Lines
);