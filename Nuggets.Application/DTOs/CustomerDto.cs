namespace Nuggets.Application.DTOs;

public record CustomerCreateDto(
    string Name,
    string? Email,
    string? Phone,
    string? Address
);

public record CustomerUpdateDto(
    string Name,
    string? Email,
    string? Phone,
    string? Address
);