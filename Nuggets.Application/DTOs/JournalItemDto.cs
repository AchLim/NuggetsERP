namespace Nuggets.Application.DTOs;

public sealed record JournalItemListDto(
    Guid Id,
    Guid JournalEntryId,
    Guid AccountId,
    string AccountCode,
    string AccountName,
    decimal Debit,
    decimal Credit,
    string? Description
);

public sealed record JournalItemReadDto(
    Guid Id,
    Guid JournalEntryId,
    Guid AccountId,
    string AccountCode,
    string AccountName,
    decimal Debit,
    decimal Credit,
    string? Description
);

public sealed record JournalItemCreateDto(
    Guid? Id,
    Guid AccountId,
    decimal Debit,
    decimal Credit,
    string? Description
);

public sealed record JournalItemUpdateDto(
    Guid? Id,
    Guid AccountId,
    decimal Debit,
    decimal Credit,
    string? Description
);