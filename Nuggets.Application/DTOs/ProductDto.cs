namespace Nuggets.Application.DTOs;

public record ProductCreateDto(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    Guid? ProductCategoryId,
    Guid? SupplierId
);

public record ProductUpdateDto(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    Guid? ProductCategoryId,
    Guid? SupplierId
);