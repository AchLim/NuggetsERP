namespace Nuggets.Application.DTOs;

public record FoodRecipeCreateDto(
    Guid ProductId,
    Guid FoodMaterialId,
    decimal Quantity,
    Guid UomId
);

public record FoodRecipeUpdateDto(
    decimal Quantity,
    Guid UomId
);