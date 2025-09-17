using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class InventoryService(
    IStockMovementRepository movementRepo,
    IProductRepository productRepo,
    IPurchaseReceiptRepository purchaseReceiptRepo,
    ISalesReceiptRepository salesReceiptRepo,
    IGoodsReceiptNoteRepository grnRepo,
    IDeliveryNoteRepository dnRepo,
    IChartOfAccountRepository coaRepo,
    IJournalEntryService journalService,
    IUomService uomService
) : IInventoryService
{
    public async Task<Result<IReadOnlyList<StockMovementDto>>> GetAllMovementsAsync(CancellationToken ct = default)
    {
        var movements = await movementRepo.GetAllAsync(ct);
        var result = movements.Select(m => new StockMovementDto(
            m.Id,
            m.Product.Name,
            m.MovementType,
            m.Quantity,
            m.MovementDate,
            m.ReferenceType,
            m.ReferenceId,
            GetReferenceUrl(m.ReferenceType, m.ReferenceId)
        )).ToList();

        return Result<IReadOnlyList<StockMovementDto>>.Ok(result);
    }

    public async Task<Result<ProductInventoryDto>> GetProductMovementsAsync(Guid productId,
        CancellationToken ct = default)
    {
        var product = await productRepo.GetWithMovementsAsync(productId, ct);
        if (product == null)
            return Result<ProductInventoryDto>.Err("Product not found", "NOT_FOUND");

        var currentStock = product.StockMovements.Sum(sm =>
            sm.MovementType switch
            {
                StockMovementType.Inbound => sm.Quantity,
                StockMovementType.Outbound => -sm.Quantity,
                _ => 0
            });

        var movements = product.StockMovements
            .OrderByDescending(sm => sm.MovementDate)
            .Select(m => new StockMovementDto(
                m.Id,
                product.Name,
                m.MovementType,
                m.Quantity,
                m.MovementDate,
                m.ReferenceType,
                m.ReferenceId,
                GetReferenceUrl(m.ReferenceType, m.ReferenceId)
            )).ToList();

        var dto = new ProductInventoryDto(product.Name, currentStock, movements);
        return Result<ProductInventoryDto>.Ok(dto);
    }

    // ===================== PURCHASE RECEIPT =====================
    public async Task<Result<bool>> ApplyPurchaseReceiptAsync(Guid purchaseReceiptId, CancellationToken ct = default)
    {
        var receipt = await purchaseReceiptRepo.GetByIdAsync(purchaseReceiptId, ct);
        if (receipt is null) return Result<bool>.Err("Purchase receipt not found");

        foreach (var line in receipt.Lines)
        {
            var product = await productRepo.GetByIdAsync(line.ProductId, ct);
            if (product == null) return Result<bool>.Err($"Product {line.ProductId} not found");

            // Current stock qty and value
            var oldQty = await movementRepo.GetNetQuantityAsync(product.Id, ct);
            var oldValue = oldQty * product.CurrentMovingAverageCost;

            // Convert purchase receipt line qty & cost into base UOM
            var (qtyInBase, unitCostInBase) = await uomService.ConvertLineAsync(
                product.Id, line.UomId, line.Quantity, line.UnitCost, ct);

            // Then use qtyInBase and unitCostInBase for all inventory + avg cost calc
            var newQty = qtyInBase;
            var unitCost = unitCostInBase;
            
            var newValue = newQty * unitCost;

            // recalc weighted average
            var newTotalQty = oldQty + newQty;
            var newTotalValue = oldValue + newValue;

            product.CurrentMovingAverageCost = newTotalQty > 0
                ? Math.Round(newTotalValue / newTotalQty, 2)
                : 0;

            await productRepo.UpdateAsync(product, ct);

            // 1. Stock Movement (Inbound)
            var movement = new StockMovement
            {
                ProductId = line.ProductId,
                MovementType = StockMovementType.Inbound,
                Quantity = newQty,
                UnitCost = unitCost,
                MovementDate = receipt.ReceiptDate,
                ReferenceId = receipt.Id,
                ReferenceType = "PurchaseReceipt"
            };

            await movementRepo.AddAsync(movement, ct);

            // 2. Journal Entry
            var invAcc = await coaRepo.GetInventoryAccountAsync(ct);
            var cashAcc = await coaRepo.GetCashOrBankAccountAsync(receipt.Method, ct);

            await journalService.PostAsync(
                $"Inventory receipt for Purchase Receipt {receipt.ReceiptNumber}",
                receipt.ReceiptDate,
                new[]
                {
                    (invAcc, newValue, 0m), // Dr Inventory
                    (cashAcc, 0m, newValue) // Cr Cash
                });
        }

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> RevertPurchaseReceiptAsync(Guid purchaseReceiptId, CancellationToken ct = default)
    {
        var receipt = await purchaseReceiptRepo.GetByIdAsync(purchaseReceiptId, ct);
        if (receipt is null) return Result<bool>.Err("Purchase receipt not found");

        var originalInbounds = await movementRepo.GetByReferenceAsync(receipt.Id, "PurchaseReceipt", ct);

        foreach (var inMov in originalInbounds)
        {
            var totalCost = inMov.Quantity * inMov.UnitCost;

            // 1. Reverse stock movement (Outbound)
            var reverseMov = new StockMovement
            {
                ProductId = inMov.ProductId,
                MovementType = StockMovementType.Outbound,
                Quantity = inMov.Quantity,
                UnitCost = inMov.UnitCost,
                MovementDate = DateTime.UtcNow,
                ReferenceId = receipt.Id,
                ReferenceType = "PurchaseReceipt-Reversal"
            };
            await movementRepo.AddAsync(reverseMov, ct);

            // 2. Reverse journal entry
            var invAcc = await coaRepo.GetInventoryAccountAsync(ct);
            var cashAcc = await coaRepo.GetCashOrBankAccountAsync(receipt.Method, ct);

            await journalService.PostAsync(
                $"Reversal of PR {receipt.ReceiptNumber}",
                DateTime.UtcNow,
                new[]
                {
                    (cashAcc, totalCost, 0m), // Dr Cash
                    (invAcc, 0m, totalCost) // Cr Inventory
                });
        }

        return Result<bool>.Ok(true);
    }

    // ===================== SALES RECEIPT =====================
    public async Task<Result<bool>> ApplySalesReceiptAsync(Guid salesReceiptId, CancellationToken ct = default)
    {
        var sale = await salesReceiptRepo.GetByIdAsync(salesReceiptId, ct);
        if (sale is null) return Result<bool>.Err("Sales receipt not found");

        foreach (var line in sale.Lines)
        {
            var product = await productRepo.GetByIdAsync(line.ProductId, ct);
            if (product == null) return Result<bool>.Err($"Product {line.ProductId} not found");
            
            // Convert sales line quantity to base UOM
            var (qtyInBase, _) = await uomService.ConvertLineAsync(
                product.Id, line.UomId, line.Quantity, product.CurrentMovingAverageCost, ct);

            var avgCost = product.CurrentMovingAverageCost;
            var totalCost = qtyInBase * avgCost;

            // 1. Stock Movement (Outbound)
            var movement = new StockMovement
            {
                ProductId = line.ProductId,
                MovementType = StockMovementType.Outbound,
                Quantity = qtyInBase,
                UnitCost = avgCost,
                MovementDate = sale.ReceiptDate,
                ReferenceId = sale.Id,
                ReferenceType = "SalesReceipt"
            };
            await movementRepo.AddAsync(movement, ct);

            // 2. Journal Entry for COGS
            var cogsAcc = await coaRepo.GetCogsAccountAsync(ct);
            var invAcc = await coaRepo.GetInventoryAccountAsync(ct);

            await journalService.PostAsync(
                $"COGS for Sales Receipt {sale.ReceiptNumber}",
                sale.ReceiptDate,
                new[]
                {
                    (cogsAcc, totalCost, 0m), // Dr COGS
                    (invAcc, 0m, totalCost) // Cr Inventory
                }
            );

            // NOTE: avg cost doesn't change on sale
        }

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> RevertSalesReceiptAsync(Guid salesReceiptId, CancellationToken ct = default)
    {
        var sale = await salesReceiptRepo.GetByIdAsync(salesReceiptId, ct);
        if (sale is null) return Result<bool>.Err("Sales receipt not found");

        var originalOutbounds = await movementRepo.GetByReferenceAsync(sale.Id, "SalesReceipt", ct);

        foreach (var outMov in originalOutbounds)
        {
            var totalCost = outMov.Quantity * outMov.UnitCost;

            // 1. Reverse stock movement (Inbound)
            var reverseMov = new StockMovement
            {
                ProductId = outMov.ProductId,
                MovementType = StockMovementType.Inbound,
                Quantity = outMov.Quantity,
                UnitCost = outMov.UnitCost,
                MovementDate = DateTime.UtcNow,
                ReferenceId = sale.Id,
                ReferenceType = "SalesReceipt-Reversal"
            };
            await movementRepo.AddAsync(reverseMov, ct);

            // 2. Reverse Journal Entry (reverse COGS)
            var cogsAcc = await coaRepo.GetCogsAccountAsync(ct);
            var invAcc = await coaRepo.GetInventoryAccountAsync(ct);

            await journalService.PostAsync(
                $"Reversal COGS for Sales Receipt {sale.ReceiptNumber}",
                DateTime.UtcNow,
                new[]
                {
                    (invAcc, totalCost, 0m), // Dr Inventory
                    (cogsAcc, 0m, totalCost) // Cr COGS
                }
            );
        }

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> ApplyGoodsReceiptAsync(Guid grnId, CancellationToken ct = default)
    {
        var grn = await grnRepo.GetByIdWithLinesAsync(grnId, ct);
        if (grn == null) return Result<bool>.Err("GRN not found", "NOT_FOUND");

        decimal totalValue = 0m;

        foreach (var line in grn.Lines)
        {
            var product = await productRepo.GetByIdAsync(line.ProductId, ct);
            if (product == null)
                return Result<bool>.Err($"Product {line.ProductId} not found");

            // lookup PO line for expected unit price
            var poLine = grn.PurchaseOrder.Lines.FirstOrDefault(x => x.ProductId == line.ProductId);
            if (poLine == null)
                return Result<bool>.Err($"No PO line found for product {line.ProductId}");

            // convert qty + cost into base UOM
            var (qtyInBase, unitCostInBase) = await uomService.ConvertLineAsync(
                product.Id, line.UomId, line.Quantity, poLine.UnitCost, ct);

            // current stock before GRN
            var oldQty = await movementRepo.GetNetQuantityAsync(product.Id, ct);
            var oldValue = oldQty * product.CurrentMovingAverageCost;

            // new GRN value
            var newValue = qtyInBase * unitCostInBase;
            totalValue += newValue;

            var newTotalQty = oldQty + qtyInBase;
            var newTotalValue = oldValue + newValue;

            // recalc moving average cost
            product.CurrentMovingAverageCost = newTotalQty > 0
                ? Math.Round(newTotalValue / newTotalQty, 2)
                : 0;
            await productRepo.UpdateAsync(product, ct);

            // record stock movement (at base UOM)
            var movement = new StockMovement
            {
                ProductId = line.ProductId,
                MovementType = StockMovementType.Inbound,
                Quantity = qtyInBase,
                UnitCost = unitCostInBase,
                MovementDate = grn.ReceiptDate,
                ReferenceId = grn.Id,
                ReferenceType = "GoodsReceiptNote"
            };
            await movementRepo.AddAsync(movement, ct);
        }

        // provisional Journal Entry (Inventory vs GRNI accrual)
        var invAcc = await coaRepo.GetInventoryAccountAsync(ct);
        var grniAcc = await coaRepo.GetGrniAccountAsync(ct);

        var journal = await journalService.PostAsync(
            $"GRN {grn.GRNNumber}",
            grn.ReceiptDate,
            new[]
            {
                (invAcc, totalValue, 0m), // Debit Inventory
                (grniAcc, 0m, totalValue) // Credit GRNI Accrual
            });

        grn.JournalEntryId = journal.Id;
        await grnRepo.UpdateAsync(grn, ct);

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> RevertGoodsReceiptAsync(Guid grnId, CancellationToken ct = default)
    {
        var grn = await grnRepo.GetByIdWithLinesAsync(grnId, ct);
        if (grn == null) return Result<bool>.Err("GRN not found", "NOT_FOUND");

        var inbounds = await movementRepo.GetByReferenceAsync(grn.Id, "GoodsReceiptNote", ct);
        decimal reversalValue = 0m;

        foreach (var movement in inbounds)
        {
            // 1. Reverse stock movement (outbound)
            var reverse = new StockMovement
            {
                ProductId = movement.ProductId,
                MovementType = StockMovementType.Outbound,
                Quantity = movement.Quantity,
                UnitCost = movement.UnitCost,
                MovementDate = DateTime.UtcNow,
                ReferenceId = grn.Id,
                ReferenceType = "GoodsReceiptNote-Reversal"
            };
            await movementRepo.AddAsync(reverse, ct);

            reversalValue += movement.Quantity * movement.UnitCost;
        }

        // 2. Reverse journal posting
        var invAcc = await coaRepo.GetInventoryAccountAsync(ct);
        var grniAcc = await coaRepo.GetGrniAccountAsync(ct);

        await journalService.PostAsync(
            $"Reversal of GRN {grn.GRNNumber}",
            DateTime.UtcNow,
            new[]
            {
                (grniAcc, reversalValue, 0m), // Dr GRNI
                (invAcc, 0m, reversalValue) // Cr Inventory
            });

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> ApplyDeliveryNoteAsync(Guid dnId, CancellationToken ct = default)
    {
        var dn = await dnRepo.GetByIdWithLinesAsync(dnId, ct);
        if (dn == null) return Result<bool>.Err("Delivery Note not found", "NOT_FOUND");

        decimal totalCost = 0m;

        foreach (var line in dn.Lines)
        {
            var product = await productRepo.GetByIdAsync(line.ProductId, ct);
            if (product == null)
                return Result<bool>.Err($"Product {line.ProductId} not found");

            // Convert quantities to base UOM, apply avg cost
            var (qtyInBase, _) = await uomService.ConvertLineAsync(
                product.Id, line.UomId, line.Quantity, product.CurrentMovingAverageCost, ct);

            var avgCost = product.CurrentMovingAverageCost;
            var cost = qtyInBase * avgCost;
            totalCost += cost;

            // Stock movement: outbound
            var movement = new StockMovement
            {
                ProductId = line.ProductId,
                MovementType = StockMovementType.Outbound,
                Quantity = qtyInBase,
                UnitCost = avgCost,
                MovementDate = dn.DeliveryDate,
                ReferenceId = dn.Id,
                ReferenceType = "DeliveryNote"
            };
            await movementRepo.AddAsync(movement, ct);
        }

        // Journal entry: COGS vs Inventory
        var cogsAcc = await coaRepo.GetCogsAccountAsync(ct);
        var invAcc = await coaRepo.GetInventoryAccountAsync(ct);

        var journal = await journalService.PostAsync(
            $"Delivery Note {dn.DeliveryNumber}",
            dn.DeliveryDate,
            new[]
            {
                (cogsAcc, totalCost, 0m), // Dr COGS
                (invAcc, 0m, totalCost) // Cr Inventory
            });

        dn.JournalEntryId = journal.Id;
        await dnRepo.UpdateAsync(dn, ct);

        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> RevertDeliveryNoteAsync(Guid dnId, CancellationToken ct = default)
    {
        var dn = await dnRepo.GetByIdWithLinesAsync(dnId, ct);
        if (dn == null) return Result<bool>.Err("Delivery Note not found", "NOT_FOUND");

        var outbounds = await movementRepo.GetByReferenceAsync(dn.Id, "DeliveryNote", ct);
        decimal reversalValue = 0m;

        foreach (var movement in outbounds)
        {
            // 1. Reverse stock movement (inbound)
            var reverse = new StockMovement
            {
                ProductId = movement.ProductId,
                MovementType = StockMovementType.Inbound,
                Quantity = movement.Quantity,
                UnitCost = movement.UnitCost,
                MovementDate = DateTime.UtcNow,
                ReferenceId = dn.Id,
                ReferenceType = "DeliveryNote-Reversal"
            };
            await movementRepo.AddAsync(reverse, ct);

            reversalValue += movement.Quantity * movement.UnitCost;
        }

        // 2. Reverse journal entry
        var cogsAcc = await coaRepo.GetCogsAccountAsync(ct);
        var invAcc = await coaRepo.GetInventoryAccountAsync(ct);

        await journalService.PostAsync(
            $"Reversal of DN {dn.DeliveryNumber}",
            DateTime.UtcNow,
            new[]
            {
                (invAcc, reversalValue, 0m), // Dr Inventory
                (cogsAcc, 0m, reversalValue) // Cr COGS
            });

        return Result<bool>.Ok(true);
    }

    private static string? GetReferenceUrl(string? referenceType, Guid? referenceId)
    {
        if (referenceType == null || referenceId == null)
            return null;

        return referenceType switch
        {
            "PurchaseReceipt" => $"/erp/purchases/receipts/{referenceId}",
            "PurchaseReceipt-Reversal" => $"/erp/purchases/receipts/{referenceId}?reversal=true",
            "SalesReceipt" => $"/erp/sales/receipts/{referenceId}",
            "SalesReceipt-Reversal" => $"/erp/sales/receipts/{referenceId}?reversal=true",
            _ => null
        };
    }
}