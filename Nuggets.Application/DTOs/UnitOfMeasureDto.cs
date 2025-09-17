namespace Nuggets.Application.DTOs;

public record UnitOfMeasureCreateDto(
    string Name,
    string Abbreviation
);

public record UnitOfMeasureUpdateDto(
    string Name,
    string Abbreviation
);

// Full detail
public record UnitOfMeasureReadDto(
    Guid Id,
    string Name,
    string Abbreviation
);

// For list view
public record UnitOfMeasureListDto(
    Guid Id,
    string Name,
    string Abbreviation
);