using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class JournalItemService(IJournalItemRepository repo, IJournalEntryRepository entryRepo) : IJournalItemService
{
    public async Task<Result<PagedResult<JournalItemListDto>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, total) = await repo.GetPagedAsync(page, pageSize);
        var list = items.Select(i => new JournalItemListDto(
            i.Id, i.JournalEntryId, i.AccountId, i.Account?.Code ?? string.Empty, i.Account?.Name ?? string.Empty,
            i.Debit, i.Credit, i.Description
        )).ToList();
        return Result<PagedResult<JournalItemListDto>>.Ok(new PagedResult<JournalItemListDto>(list, total, page, pageSize));
    }

    public async Task<Result<JournalItemReadDto>> GetByIdAsync(Guid id)
    {
        var item = await repo.GetByIdAsync(id);
        return item is not null
            ? Result<JournalItemReadDto>.Ok(new JournalItemReadDto(item.Id, item.JournalEntryId, item.AccountId, item.Account?.Code ?? string.Empty, item.Account?.Name ?? string.Empty, item.Debit, item.Credit, item.Description))
            : Result<JournalItemReadDto>.Err("Journal item not found", "NOT_FOUND");
    }

    public async Task<Result<JournalItemReadDto>> UpdateAsync(Guid id, JournalItemUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<JournalItemReadDto>.Err("Journal item not found", "NOT_FOUND");

            existing.Debit = dto.Debit;
            existing.Credit = dto.Credit;
            existing.Description = dto.Description;

            // After update ensure parent journal entry still balanced
            var parent = await entryRepo.GetByIdAsync(existing.JournalEntryId);
            if (parent is null)
                throw new InvalidOperationException("Parent journal entry not found");

            await repo.UpdateAsync(existing);

            // reload items for balance check
            var items = parent.Items.ToList();
            // update the item in-memory to use new values for check
            var memItem = items.First(i => i.Id == existing.Id);
            memItem.Debit = existing.Debit;
            memItem.Credit = existing.Credit;

            var totalDebit = items.Sum(i => i.Debit);
            var totalCredit = items.Sum(i => i.Credit);
            if (totalDebit != totalCredit)
                throw new InvalidOperationException("Journal entry becomes unbalanced after update");

            await tx.CommitAsync();
            return Result<JournalItemReadDto>.Ok(new JournalItemReadDto(existing.Id, existing.JournalEntryId, existing.AccountId, existing.Account?.Code ?? string.Empty, existing.Account?.Name ?? string.Empty, existing.Debit, existing.Credit, existing.Description));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<JournalItemReadDto>.Err($"Failed to update journal item: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<bool>.Err("Journal item not found", "NOT_FOUND");

            var parent = await entryRepo.GetByIdAsync(existing.JournalEntryId);
            if (parent is null)
                throw new InvalidOperationException("Parent journal entry not found");

            await repo.DeleteAsync(existing);

            // ensure parent still balanced after deletion
            var remainingItems = parent.Items.Where(i => i.Id != id).ToList();
            var totalDebit = remainingItems.Sum(i => i.Debit);
            var totalCredit = remainingItems.Sum(i => i.Credit);
            if (totalDebit != totalCredit)
                throw new InvalidOperationException("Deleting this item would unbalance the journal entry");

            await tx.CommitAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<bool>.Err($"Failed to delete journal item: {ex.Message}", "DB_ERROR");
        }
    }
}
