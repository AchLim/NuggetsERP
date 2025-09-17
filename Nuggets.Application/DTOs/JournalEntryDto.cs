using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public sealed record JournalEntryCreateDto(
    DateTime EntryDate,
    string? Reference,
    List<JournalItemCreateDto> Items
);

public sealed record JournalEntryUpdateDto(
    DateTime EntryDate,
    string? Reference,
    JournalEntryStatus Status,
    List<JournalItemCreateDto> Items
);

public sealed record JournalEntryListDto(
    Guid Id,
    string? EntryNumber,
    DateTime EntryDate,
    string? Reference,
    decimal TotalDebit,
    decimal TotalCredit,
    string Status
);

public sealed record JournalEntryReadDto(
    Guid Id,
    string? EntryNumber,
    DateTime EntryDate,
    string? Reference,
    List<JournalItemReadDto> Items,
    string Status
);