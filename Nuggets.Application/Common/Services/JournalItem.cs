using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IJournalItemService
{
    Task<Result<PagedResult<JournalItemListDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<JournalItemReadDto>> GetByIdAsync(Guid id);
    Task<Result<JournalItemReadDto>> UpdateAsync(Guid id, JournalItemUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}