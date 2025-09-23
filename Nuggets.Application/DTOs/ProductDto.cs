namespace Nuggets.Application.DTOs;

public record ProductCreateDto(
    string Name,
    string? Description,
    Guid UomId,
    decimal DefaultPurchasePrice,
    decimal DefaultSalesPrice,
    Guid? ProductCategoryId,
    Guid? VendorId
);

public record ProductUpdateDto(
    string Name,
    string? Description,
    Guid UomId,
    decimal DefaultPurchasePrice,
    decimal DefaultSalesPrice,
    Guid? ProductCategoryId,
    Guid? VendorId
);

// Full detail
public record ProductReadDto(
    Guid Id,
    string Name,
    string? Description,
    Guid UomId,
    string UomName,
    decimal DefaultPurchasePrice,
    decimal DefaultSalesPrice,
    Guid? ProductCategoryId,
    string? CategoryName,
    Guid? VendorId,
    string? VendorName,
    decimal CurrentStock,  // computed from StockMovements
    List<StockMovementReadDto> StockMovements
);

public record StockMovementReadDto(
    Guid Id,
    DateTime MovementDate,
    string MovementType,
    decimal Quantity,
    string? ReferenceType,
    Guid? ReferenceId
);

// For list view
public record ProductListDto(
    Guid Id,
    string Name,
    decimal DefaultPurchasePrice,
    decimal DefaultSalesPrice,
    string? CategoryName,
    Guid UomId,
    string UomName,
    string? VendorName,
    decimal CurrentStock
);