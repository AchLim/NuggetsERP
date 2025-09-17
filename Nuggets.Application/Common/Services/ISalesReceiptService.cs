using Nuggets.Application.DTOs;

namespace Nuggets.Application.Common.Services;

public interface ISalesReceiptService
{
    Task<Result<PagedResult<SalesReceiptListDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<IReadOnlyList<SalesReceiptListDto>>> GetAllAsync();
    Task<Result<SalesReceiptReadDto>> GetByIdAsync(Guid id);
    Task<Result<SalesReceiptReadDto>> CreateAsync(SalesReceiptCreateDto dto);
    Task<Result<SalesReceiptReadDto>> UpdateAsync(Guid id, SalesReceiptUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}