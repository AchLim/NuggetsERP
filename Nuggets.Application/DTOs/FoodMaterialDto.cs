namespace Nuggets.Application.DTOs;

public record FoodMaterialCreateDto(
    string Name,
    decimal PricePerUnit,
    Guid UomId
);

public record FoodMaterialUpdateDto(
    string Name,
    decimal PricePerUnit,
    Guid UomId,
    bool Active
);