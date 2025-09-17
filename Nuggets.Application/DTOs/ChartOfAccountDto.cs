// ChartOfAccountDtos.cs

using System.Text.Json.Serialization;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public sealed record ChartOfAccountListDto(
    Guid Id,
    string Code,
    string Name,
    string Type
);

public sealed record ChartOfAccountReadDto(
    Guid Id,
    string Code,
    string Name,
    string Type
);

public sealed record ChartOfAccountCreateDto(
    string Code,
    string Name,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] AccountType Type
);

public sealed record ChartOfAccountUpdateDto(
    string Code,
    string Name,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] AccountType Type
);