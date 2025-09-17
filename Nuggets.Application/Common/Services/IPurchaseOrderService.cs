using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IPurchaseOrderService
{
    Task<Result<PagedResult<PurchaseOrderListDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<IReadOnlyList<PurchaseOrderListDto>>> GetAllAsync();
    Task<Result<PurchaseOrderReadDto>> GetByIdAsync(Guid id);
    Task<Result<PurchaseOrderReadDto>> CreateAsync(PurchaseOrderCreateDto dto);
    Task<Result<PurchaseOrderReadDto>> UpdateAsync(Guid id, PurchaseOrderUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
