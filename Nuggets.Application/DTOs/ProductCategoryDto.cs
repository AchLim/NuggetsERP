namespace Nuggets.Application.DTOs;

public record ProductCategoryCreateDto(
    string Name,
    bool? Active,
    int Sequence,
    Guid? ParentId
);

public record ProductCategoryUpdateDto(
    string Name,
    bool Active,
    int Sequence,
    Guid? ParentId = null
);