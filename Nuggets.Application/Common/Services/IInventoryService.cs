using Nuggets.Application.Common;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Services;

public interface IInventoryService
{
    Task<Result<IReadOnlyList<StockMovementDto>>> GetAllMovementsAsync(CancellationToken ct = default);
    Task<Result<ProductInventoryDto>> GetProductMovementsAsync(Guid productId, CancellationToken ct = default);

    Task<Result<bool>> ApplyPurchaseReceiptAsync(Guid purchaseReceiptId, CancellationToken ct = default);
    Task<Result<bool>> RevertPurchaseReceiptAsync(Guid purchaseReceiptId, CancellationToken ct = default);
    Task<Result<bool>> ApplySalesReceiptAsync(Guid salesReceiptId, CancellationToken ct = default);
    Task<Result<bool>> RevertSalesReceiptAsync(Guid salesReceiptId, CancellationToken ct = default);
    
    Task<Result<bool>> ApplyGoodsReceiptAsync(Guid grnId, CancellationToken ct = default);
    Task<Result<bool>> RevertGoodsReceiptAsync(Guid grnId, CancellationToken ct = default);
    Task<Result<bool>> ApplyDeliveryNoteAsync(Guid dnId, CancellationToken ct = default);
    Task<Result<bool>> RevertDeliveryNoteAsync(Guid dnId, CancellationToken ct = default);
}