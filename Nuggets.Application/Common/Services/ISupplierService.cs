using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IVendorService
{
    Task<Result<PagedResult<Vendor>>> GetPagedAsync(int page, int pageSize);
    Task<Result<IReadOnlyList<Vendor>>> GetAllAsync();
    Task<Result<Vendor>> GetByIdAsync(Guid id);
    Task<Result<Vendor>> CreateAsync(VendorCreateDto dto);
    Task<Result<Vendor>> UpdateAsync(Guid id, VendorUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
