using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class JournalEntryService(IJournalEntryRepository repo, IChartOfAccountRepository accountRepo) : IJournalEntryService
{
    public async Task<Result<PagedResult<JournalEntryListDto>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize);
        var list = items.Select(ToListDto).ToList();

        return Result<PagedResult<JournalEntryListDto>>.Ok(new PagedResult<JournalEntryListDto>(list, totalCount, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<JournalEntryListDto>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return Result<IReadOnlyList<JournalEntryListDto>>.Ok(list.Select(ToListDto).ToList());
    }

    public async Task<Result<JournalEntryReadDto>> GetByIdAsync(Guid id)
    {
        var entity = await repo.GetByIdAsync(id);
        return entity is not null
            ? Result<JournalEntryReadDto>.Ok(ToReadDto(entity))
            : Result<JournalEntryReadDto>.Err("Journal entry not found", "NOT_FOUND");
    }

    public async Task<Result<JournalEntryReadDto>> CreateAsync(JournalEntryCreateDto dto)
    {
        if (dto.Items.Count == 0)
            return Result<JournalEntryReadDto>.Err("Journal entry must have at least one item", "VALIDATION_ERROR");

        var totalDebit = dto.Items.Sum(i => i.Debit);
        var totalCredit = dto.Items.Sum(i => i.Credit);
        if (totalDebit != totalCredit)
            return Result<JournalEntryReadDto>.Err("Total debit must equal total credit", "BALANCE_ERROR");

        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var draftLabel = $"Draft JE *{DateTime.UtcNow:yyyyMMddHHmmss}";

            var entity = new JournalEntry
            {
                EntryNumber = draftLabel,
                EntryDate = dto.EntryDate,
                Reference = dto.Reference,
                Items = dto.Items.Select(i => new JournalItem
                {
                    AccountId = i.AccountId,
                    Debit = i.Debit,
                    Credit = i.Credit,
                    Description = i.Description
                }).ToList()
            };

            // Optional: validate each AccountId exists
            var accountIds = entity.Items.Select(it => it.AccountId).Distinct().ToList();
            var accounts = (await accountRepo.FindAsync(a => accountIds.Contains(a.Id))).ToDictionary(a => a.Id);
            foreach (var it in entity.Items)
            {
                if (!accounts.ContainsKey(it.AccountId))
                    throw new InvalidOperationException($"Account {it.AccountId} not found");
            }

            await repo.AddAsync(entity);
            await tx.CommitAsync();
            return Result<JournalEntryReadDto>.Ok(ToReadDto(entity));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<JournalEntryReadDto>.Err($"Failed to create journal entry: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<JournalEntryReadDto>> UpdateAsync(Guid id, JournalEntryUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<JournalEntryReadDto>.Err("Journal entry not found", "NOT_FOUND");

            // Validate balancing
            var totalDebit = dto.Items.Sum(i => i.Debit);
            var totalCredit = dto.Items.Sum(i => i.Credit);
            if (totalDebit != totalCredit)
                return Result<JournalEntryReadDto>.Err("Total debit must equal total credit", "BALANCE_ERROR");


            if (dto.Status == JournalEntryStatus.Posted && 
                (string.IsNullOrEmpty(existing.EntryNumber) || existing.EntryNumber.StartsWith("Draft JE")))
            {
                // Generate auto number
                var nextNumber = await repo.GetNextSequenceValueAsync("journal_entry_number_seq");
                existing.EntryNumber = $"JE/{dto.EntryDate.Year}/{nextNumber:000000}";
            }
            
            existing.EntryDate = dto.EntryDate;
            existing.Reference = dto.Reference;
            existing.Status = dto.Status;

            // Replace items
            existing.Items.Clear();
            foreach (var it in dto.Items)
            {
                existing.Items.Add(new JournalItem
                {
                    AccountId = it.AccountId,
                    Debit = it.Debit,
                    Credit = it.Credit,
                    Description = it.Description
                });
            }

            await repo.UpdateAsync(existing);
            await tx.CommitAsync();
            return Result<JournalEntryReadDto>.Ok(ToReadDto(existing));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<JournalEntryReadDto>.Err($"Failed to update journal entry: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<bool>.Err("Journal entry not found", "NOT_FOUND");

            await repo.DeleteAsync(existing);
            await tx.CommitAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<bool>.Err($"Failed to delete journal entry: {ex.Message}", "DB_ERROR");
        }
    }

    private static JournalEntryListDto ToListDto(JournalEntry e) =>
        new JournalEntryListDto(
            e.Id, e.EntryNumber, e.EntryDate, e.Reference, e.Items.Sum(i => i.Debit), e.Items.Sum(i => i.Credit), e.Status.ToString()
        );

    private static JournalEntryReadDto ToReadDto(JournalEntry e) =>
        new JournalEntryReadDto(e.Id, e.EntryNumber, e.EntryDate, e.Reference,
            e.Items.Select(i => new JournalItemReadDto(
                i.Id, e.Id, i.AccountId, i.Account?.Code ?? string.Empty, i.Account?.Name ?? string.Empty, i.Debit, i.Credit, i.Description)
            ).ToList(),
            e.Status.ToString()
        );
    
    public async Task<JournalEntry> PostAsync(
        string reference,
        DateTime entryDate,
        (ChartOfAccount account, decimal debit, decimal credit)[] lines)
    {
        var nextEntryNumber = await repo.GetNextSequenceValueAsync("journal_entry_number_seq");
        var je = new JournalEntry
        {
            EntryNumber = $"JE/{entryDate.Year}/{nextEntryNumber:000000}",
            EntryDate = entryDate,
            Reference = reference,
            Status = JournalEntryStatus.Posted,
            Items = lines
                .Where(l => l.debit != 0 || l.credit != 0)
                .Select(l => new JournalItem
            {
                AccountId = l.account.Id,
                Debit = l.debit,
                Credit = l.credit
            }).ToList()
        };

        await repo.AddAsync(je);
        return je;
    }

    public async Task<JournalEntry> ReverseAsync(JournalEntry original, string reason)
    {
        var nextEntryNumber = await repo.GetNextSequenceValueAsync("journal_entry_number_seq");
        var reversal = new JournalEntry
        {
            EntryNumber = $"JE/{original.EntryDate.Year}/{nextEntryNumber:000000}",
            EntryDate = DateTime.UtcNow,
            Reference = $"Reversal ({reason}) of {original.EntryNumber}",
            Status = JournalEntryStatus.Posted,
            Items = original.Items.Select(i => new JournalItem
            {
                AccountId = i.AccountId,
                Debit = i.Credit,
                Credit = i.Debit
            }).ToList()
        };

        await repo.AddAsync(reversal);
        return reversal;
    }
}
