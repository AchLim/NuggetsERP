using Nuggets.Application.DTOs;

namespace Nuggets.Application.Common.Services;

public interface IDeliveryNoteService
{
    Task<Result<PagedResult<DeliveryNoteListDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<DeliveryNoteReadDto>> GetByIdAsync(Guid id);
    Task<Result<DeliveryNoteReadDto>> CreateAsync(DeliveryNoteCreateDto dto);
    Task<Result<DeliveryNoteReadDto>> UpdateAsync(Guid id, DeliveryNoteUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}