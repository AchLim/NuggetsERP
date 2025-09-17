using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface ICustomerPaymentService
{
    Task<Result<PagedResult<CustomerPaymentListDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<IReadOnlyList<CustomerPaymentListDto>>> GetAllAsync();
    Task<Result<CustomerPaymentReadDto>> GetByIdAsync(Guid id);
    Task<Result<CustomerPaymentReadDto>> CreateAsync(CustomerPaymentCreateDto dto);
    Task<Result<CustomerPaymentReadDto>> UpdateAsync(Guid id, CustomerPaymentUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
