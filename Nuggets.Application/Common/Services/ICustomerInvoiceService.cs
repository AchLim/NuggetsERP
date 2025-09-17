using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface ICustomerInvoiceService
{
    Task<Result<PagedResult<CustomerInvoiceListDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<IReadOnlyList<CustomerInvoiceListDto>>> GetAllAsync();
    Task<Result<CustomerInvoiceReadDto>> GetByIdAsync(Guid id);
    Task<Result<CustomerInvoiceReadDto>> CreateAsync(CustomerInvoiceCreateDto dto);
    Task<Result<CustomerInvoiceReadDto>> UpdateAsync(Guid id, CustomerInvoiceUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
