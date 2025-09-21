using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IVendorBillService
{
    Task<Result<PagedResult<VendorBillListDto>>> GetPagedAsync(int page, int pageSize, Guid? purchaseOrderId = null);
    Task<Result<IReadOnlyList<VendorBillListDto>>> GetAllAsync();
    Task<Result<VendorBillReadDto>> GetByIdAsync(Guid id);
    Task<Result<VendorBillReadDto>> CreateAsync(VendorBillCreateDto dto);
    Task<Result<VendorBillReadDto>> UpdateAsync(Guid id, VendorBillUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
