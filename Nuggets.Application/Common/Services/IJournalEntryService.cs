using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IJournalEntryService
{
    Task<Result<PagedResult<JournalEntryListDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<IReadOnlyList<JournalEntryListDto>>> GetAllAsync();
    Task<Result<JournalEntryReadDto>> GetByIdAsync(Guid id);
    Task<Result<JournalEntryReadDto>> CreateAsync(JournalEntryCreateDto dto);
    Task<Result<JournalEntryReadDto>> UpdateAsync(Guid id, JournalEntryUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
    
    
    Task<JournalEntry> PostAsync(
        string reference,
        DateTime entryDate,
        (ChartOfAccount account, decimal debit, decimal credit)[] lines);

    Task<JournalEntry> ReverseAsync(JournalEntry original, string reason);
}