using Nuggets.Application.DTOs;

namespace Nuggets.Application.Common.Services;

public interface ISalesOrderService
{
    Task<Result<PagedResult<SalesOrderListDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<IReadOnlyList<SalesOrderListDto>>> GetAllAsync();
    Task<Result<SalesOrderReadDto>> GetByIdAsync(Guid id);
    Task<Result<SalesOrderReadDto>> CreateAsync(SalesOrderCreateDto dto);
    Task<Result<SalesOrderReadDto>> UpdateAsync(Guid id, SalesOrderUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}