using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class SalesReceiptService(
    ISalesReceiptRepository repo,
    IStockMovementRepository stockRepo,
    IInventoryService inventoryService,
    IChartOfAccountRepository coaRepo,
    IJournalEntryRepository journalRepo,
    IJournalEntryService journalService
) : ISalesReceiptService
{
    public async Task<Result<PagedResult<SalesReceiptListDto>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, total) = await repo.GetPagedAsync(page, pageSize);
        var list = items.Select(ToListDto).ToList();
        return Result<PagedResult<SalesReceiptListDto>>.Ok(new PagedResult<SalesReceiptListDto>(list, total, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<SalesReceiptListDto>>> GetAllAsync()
    {
        var items = await repo.GetAllAsync();
        return Result<IReadOnlyList<SalesReceiptListDto>>.Ok(items.Select(ToListDto).ToList());
    }

    public async Task<Result<SalesReceiptReadDto>> GetByIdAsync(Guid id)
    {
        var ent = await repo.GetByIdAsync(id);
        return ent is not null ? Result<SalesReceiptReadDto>.Ok(ToReadDto(ent)) : Result<SalesReceiptReadDto>.Err("Sales receipt not found", "NOT_FOUND");
    }

    public async Task<Result<SalesReceiptReadDto>> CreateAsync(SalesReceiptCreateDto dto)
    {
        if (dto.Lines.Count == 0) return Result<SalesReceiptReadDto>.Err("At least one line required", "VALIDATION_ERROR");

        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var draftLabel = $"Draft SR *{DateTime.UtcNow:yyyyMMddHHmmss}";
            
            var ent = new SalesReceipt
            {
                CustomerId = dto.CustomerId,
                ReceiptNumber = draftLabel,
                ReceiptDate = dto.ReceiptDate,
                Status = SalesReceiptStatus.Draft,
                Method = dto.Method,
                Lines = dto.Lines.Select(l => new SalesReceiptLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    DiscountPercent = l.DiscountPercent
                }).ToList()
            };

            await repo.AddAsync(ent);

            await tx.CommitAsync();
            return Result<SalesReceiptReadDto>.Ok(ToReadDto(ent));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<SalesReceiptReadDto>.Err($"Failed to create receipt: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<SalesReceiptReadDto>> UpdateAsync(Guid id, SalesReceiptUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<SalesReceiptReadDto>.Err("Sales receipt not found", "NOT_FOUND");
            
            existing.CustomerId = dto.CustomerId;
            existing.ReceiptDate = dto.ReceiptDate;
            existing.Method = dto.Method;

            existing.Lines.Clear();
            foreach (var l in dto.Lines)
            {
                existing.Lines.Add(new SalesReceiptLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    DiscountPercent = l.DiscountPercent
                });
            }

            if (dto.Status == SalesReceiptStatus.Posted && 
                (string.IsNullOrEmpty(existing.ReceiptNumber) || existing.ReceiptNumber.StartsWith("Draft SR")))
            {
                // Generate auto number
                var nextNumber = await repo.GetNextSequenceValueAsync("sales_receipt_number_seq");
                existing.ReceiptNumber = $"SR/{dto.ReceiptDate.Year}/{nextNumber:000000}";
            }
            
            if (dto.Status == SalesReceiptStatus.Posted && existing.Status != SalesReceiptStatus.Posted)
            {
                var cashAcc = await coaRepo.GetCashOrBankAccountAsync(dto.Method);
                var revAcc  = await coaRepo.GetRevenueAccountAsync();

                var total = existing.Lines.Sum(l => l.LineTotal);

                var je = await journalService.PostAsync(
                    $"Sales Receipt {existing.ReceiptNumber}",
                    dto.ReceiptDate,
                    new[]
                    {
                        (cashAcc, total, 0m),
                        (revAcc, 0m, total)
                    });
                existing.JournalEntryId = je.Id;

                // Delegate auto stock movement + COGS posting
                await inventoryService.ApplySalesReceiptAsync(existing.Id);
            }
            else if (dto.Status == SalesReceiptStatus.Cancelled && existing.Status == SalesReceiptStatus.Posted)
            {
                if (existing.JournalEntryId.HasValue)
                {
                    var je = await journalRepo.GetByIdAsync(existing.JournalEntryId.Value);
                    if (je != null) await journalService.ReverseAsync(je, "Sales Receipt cancelled");
                }
                await inventoryService.RevertSalesReceiptAsync(existing.Id);
            }
            
            existing.Status = dto.Status;

            await repo.UpdateAsync(existing);
            await tx.CommitAsync();
            return Result<SalesReceiptReadDto>.Ok(ToReadDto(existing));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<SalesReceiptReadDto>.Err($"Failed to update receipt: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<bool>.Err("Sales receipt not found", "NOT_FOUND");
            await repo.DeleteAsync(existing);
            await tx.CommitAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<bool>.Err($"Failed to delete: {ex.Message}", "DB_ERROR");
        }
    }

    private static SalesReceiptListDto ToListDto(SalesReceipt r) =>
        new(r.Id, r.CustomerId, r.Customer?.Name, r.ReceiptNumber, r.ReceiptDate, r.Status, r.Method, r.Lines.Sum(l => l.LineTotal));

    private static SalesReceiptReadDto ToReadDto(SalesReceipt r) =>
        new(r.Id, r.CustomerId, r.Customer?.Name, r.ReceiptNumber, r.ReceiptDate, r.Status, r.Method,
            r.Lines.Select(l => new SalesReceiptLineReadDto(l.Id, l.ProductId, l.Product?.Name, l.UomId, l.Quantity, l.UnitPrice, l.DiscountPercent, l.LineTotal)).ToList());
}
