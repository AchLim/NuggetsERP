using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public sealed record CustomerInvoiceListDto(Guid Id, Guid? SalesOrderId, string? SalesOrderNumber, Guid CustomerId, string? CustomerName, string? InvoiceNumber, DateTime InvoiceDate, DateTime DueDate, string Status, decimal TotalAmount);
public sealed record CustomerInvoiceLineReadDto(Guid Id, Guid ProductId, string? ProductName, Guid UomId, decimal Quantity, decimal UnitPrice, decimal LineTotal);
public sealed record CustomerInvoiceReadDto(Guid Id, Guid? SalesOrderId, string? SalesOrderNumber, Guid CustomerId, string? CustomerName, string? InvoiceNumber, DateTime InvoiceDate, DateTime DueDate, CustomerInvoiceStatus Status, List<CustomerInvoiceLineReadDto> Lines);
public sealed record CustomerInvoiceLineCreateDto(Guid ProductId, Guid UomId, decimal Quantity, decimal UnitPrice);
public sealed record CustomerInvoiceCreateDto(Guid? SalesOrderId, string? SalesOrderNumber, Guid CustomerId, DateTime InvoiceDate, DateTime DueDate, List<CustomerInvoiceLineCreateDto> Lines);
public sealed record CustomerInvoiceLineUpdateDto(Guid Id, Guid ProductId, Guid UomId, decimal Quantity, decimal UnitPrice);
public sealed record CustomerInvoiceUpdateDto(Guid? SalesOrderId, string? SalesOrderNumber, Guid CustomerId, DateTime InvoiceDate, DateTime DueDate, CustomerInvoiceStatus Status, List<CustomerInvoiceLineUpdateDto> Lines);