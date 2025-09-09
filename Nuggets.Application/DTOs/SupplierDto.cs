namespace Nuggets.Application.DTOs;

public record SupplierCreateDto(
    string Name,
    string? Email,
    string? Phone,
    string? Address
);

public record SupplierUpdateDto(
    string Name,
    string? Email,
    string? Phone,
    string? Address
);