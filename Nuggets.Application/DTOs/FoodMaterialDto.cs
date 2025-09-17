namespace Nuggets.Application.DTOs;

public record FoodMaterialCreateDto(
    string Name,
    decimal UnitPrice,
    Guid UomId
);

public record FoodMaterialUpdateDto(
    string Name,
    decimal UnitPrice,
    Guid UomId,
    bool Active
);