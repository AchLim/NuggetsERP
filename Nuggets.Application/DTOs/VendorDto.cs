namespace Nuggets.Application.DTOs;

public record VendorCreateDto(
    string Name,
    string? Email,
    string? Phone,
    string? Address
);

public record VendorUpdateDto(
    string Name,
    string? Email,
    string? Phone,
    string? Address
);