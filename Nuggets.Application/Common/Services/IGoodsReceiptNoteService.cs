using Nuggets.Application.DTOs;

namespace Nuggets.Application.Common.Services;

public interface IGoodsReceiptNoteService
{
    Task<Result<PagedResult<GoodsReceiptNoteListDto>>> GetPagedAsync(int page, int pageSize, Guid? purchaseOrderId = null);
    Task<Result<GoodsReceiptNoteReadDto>> GetByIdAsync(Guid id);
    Task<Result<GoodsReceiptNoteReadDto>> CreateAsync(GoodsReceiptNoteCreateDto dto);
    Task<Result<GoodsReceiptNoteReadDto>> UpdateAsync(Guid id, GoodsReceiptNoteUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}