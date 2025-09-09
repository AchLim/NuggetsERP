using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface ICustomerService
{
    Task<Result<PagedResult<Customer>>> GetPagedAsync(int page, int pageSize, IDictionary<string, string?>? filters, string? sort);
    Task<Result<IReadOnlyList<Customer>>> GetAllAsync();
    Task<Result<Customer>> GetByIdAsync(Guid id);
    Task<Result<Customer>> CreateAsync(CustomerCreateDto dto);
    Task<Result<Customer>> UpdateAsync(Guid id, CustomerUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
