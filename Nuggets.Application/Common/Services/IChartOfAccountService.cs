using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IChartOfAccountService
{
    Task<Result<PagedResult<ChartOfAccountListDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<IReadOnlyList<ChartOfAccountListDto>>> GetAllAsync();
    Task<Result<ChartOfAccountReadDto>> GetByIdAsync(Guid id);
    Task<Result<ChartOfAccountReadDto>> CreateAsync(ChartOfAccountCreateDto dto);
    Task<Result<ChartOfAccountReadDto>> UpdateAsync(Guid id, ChartOfAccountUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}