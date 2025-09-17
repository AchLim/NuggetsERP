using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IPurchaseReceiptService
{
    Task<Result<PagedResult<PurchaseReceiptListDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<IReadOnlyList<PurchaseReceiptListDto>>> GetAllAsync();
    Task<Result<PurchaseReceiptReadDto>> GetByIdAsync(Guid id);
    Task<Result<PurchaseReceiptReadDto>> CreateAsync(PurchaseReceiptCreateDto dto);
    Task<Result<PurchaseReceiptReadDto>> UpdateAsync(Guid id, PurchaseReceiptUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
