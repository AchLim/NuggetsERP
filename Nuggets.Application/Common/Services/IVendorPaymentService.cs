using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IVendorPaymentService
{
    Task<Result<PagedResult<VendorPaymentListDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<IReadOnlyList<VendorPaymentListDto>>> GetAllAsync();
    Task<Result<VendorPaymentReadDto>> GetByIdAsync(Guid id);
    Task<Result<VendorPaymentReadDto>> CreateAsync(VendorPaymentCreateDto dto);
    Task<Result<VendorPaymentReadDto>> UpdateAsync(Guid id, VendorPaymentUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
