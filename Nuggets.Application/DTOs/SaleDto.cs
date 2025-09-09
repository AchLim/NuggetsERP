namespace Nuggets.Application.DTOs;

public record SaleCreateDto(
    Guid ProductId,
    int Quantity,
    decimal PricePerUnit,
    DateTime SaleDate
);

public record SaleUpdateDto(
    int Quantity,
    decimal PricePerUnit,
    DateTime SaleDate
);