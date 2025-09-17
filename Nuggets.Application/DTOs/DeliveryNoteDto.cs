// Delivery Note DTOs
using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public record DeliveryNoteListDto(
    Guid Id,
    string DeliveryNumber,
    Guid SalesOrderId,
    string SalesOrderNumber,
    Guid CustomerId,
    string CustomerName,
    DateTime DeliveryDate,
    DeliveryNoteStatus Status
);

public record DeliveryNoteLineDto(
    Guid ProductId,
    string ProductName,
    Guid UomId,
    string UomAbbreviation,
    decimal Quantity
);

public record DeliveryNoteCreateDto(
    Guid SalesOrderId,
    DateTime DeliveryDate,
    List<DeliveryNoteLineDto> Lines
);

public record DeliveryNoteUpdateDto(
    DateTime DeliveryDate,
    List<DeliveryNoteLineDto> Lines,
    DeliveryNoteStatus Status
);

public record DeliveryNoteReadDto(
    Guid Id,
    string DeliveryNumber,
    Guid SalesOrderId,
    string SalesOrderNumber,
    Guid CustomerId,
    string CustomerName,
    DateTime DeliveryDate,
    DeliveryNoteStatus Status,
    List<DeliveryNoteLineDto> Lines
);