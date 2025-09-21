using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public record GoodsReceiptNoteLineDto(
    Guid ProductId,
    string? ProductName,
    Guid UomId,
    string? UomAbbreviation,
    decimal Quantity
);

public record GoodsReceiptNoteCreateDto(
    Guid PurchaseOrderId,
    DateTime ReceiptDate,
    List<GoodsReceiptNoteLineDto> Lines
);

public record GoodsReceiptNoteUpdateDto(
    DateTime ReceiptDate,
    List<GoodsReceiptNoteLineDto> Lines,
    GoodsReceiptNoteStatus Status
);

public record GoodsReceiptNoteReadDto(
    Guid Id,
    string GRNNumber,
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid VendorId,
    string VendorName,
    DateTime ReceiptDate,
    GoodsReceiptNoteStatus Status,
    List<GoodsReceiptNoteLineDto> Lines
);

public record GoodsReceiptNoteListDto(
    Guid Id,
    string GRNNumber,
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid VendorId,
    string VendorName,
    DateTime ReceiptDate,
    GoodsReceiptNoteStatus Status
);