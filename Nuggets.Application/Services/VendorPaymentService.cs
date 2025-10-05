using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class VendorPaymentService(
    IVendorPaymentRepository repo,
    IVendorBillRepository billRepo,
    IChartOfAccountRepository coaRepo,
    IJournalEntryRepository journalRepo,
    IJournalEntryService journalService
) : IVendorPaymentService
{
    public async Task<Result<PagedResult<VendorPaymentListDto>>> GetPagedAsync(int page, int pageSize, Guid? vendorBillId = null)
    {
        
        var result = vendorBillId.HasValue
            ? await repo.GetPagedAsync(page, pageSize, repo.Query().Where(vb => vb.VendorBillId == vendorBillId.Value))
            : await repo.GetPagedAsync(page, pageSize);

        var list = result.Items.Select(p => new VendorPaymentListDto(p.Id, p.VendorBill.VendorId, p.VendorBill.Vendor.Name,
            p.VendorBillId, p.VendorBill.BillNumber, p.PaymentNumber, p.PaymentDate, p.Amount, p.Method, p.Status.ToString())).ToList();
        return Result<PagedResult<VendorPaymentListDto>>.Ok(
            new PagedResult<VendorPaymentListDto>(list, result.TotalCount, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<VendorPaymentListDto>>> GetAllAsync()
    {
        var items = await repo.GetAllAsync();
        return Result<IReadOnlyList<VendorPaymentListDto>>.Ok(items.Select(p =>
            new VendorPaymentListDto(p.Id, p.VendorBill.VendorId, p.VendorBill.Vendor.Name, p.VendorBillId, p.VendorBill.BillNumber, p.PaymentNumber, p.PaymentDate, p.Amount,
                p.Method, p.Status.ToString())).ToList());
    }

    public async Task<Result<VendorPaymentReadDto>> GetByIdAsync(Guid id)
    {
        var ent = await repo.GetByIdAsync(id);
        return ent is not null
            ? Result<VendorPaymentReadDto>.Ok(new VendorPaymentReadDto(ent.Id, ent.VendorBill.VendorId,
                ent.VendorBill.Vendor.Name, ent.VendorBillId, ent.VendorBill.BillNumber, ent.PaymentNumber, ent.PaymentDate, ent.Amount, ent.Method, ent.Status.ToString()))
            : Result<VendorPaymentReadDto>.Err("Payment not found", "NOT_FOUND");
    }

    public async Task<Result<VendorPaymentReadDto>> CreateAsync(VendorPaymentCreateDto dto)
    {
        if (dto.Amount <= 0) return Result<VendorPaymentReadDto>.Err("Amount must be positive", "VALIDATION_ERROR");
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var bill = await billRepo.GetByIdAsync(dto.VendorBillId);
            if (bill is null) return Result<VendorPaymentReadDto>.Err("Vendor bill not found", "NOT_FOUND");

            var draftLabel = $"Draft VP *{DateTime.UtcNow:yyyyMMddHHmmss}";

            var ent = new VendorPayment
            {
                VendorBillId = dto.VendorBillId,
                PaymentNumber = draftLabel,
                PaymentDate = dto.PaymentDate,
                Amount = dto.Amount,
                Method = dto.Method,
                Status = VendorPaymentStatus.Draft
            };

            await repo.AddAsync(ent);

            await tx.CommitAsync();
            return await GetByIdAsync(ent.Id);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<VendorPaymentReadDto>.Err($"Failed to create vendor payment: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<VendorPaymentReadDto>> UpdateAsync(Guid id, VendorPaymentUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<VendorPaymentReadDto>.Err("Payment not found", "NOT_FOUND");

            var bill = await billRepo.GetByIdAsync(existing.VendorBillId);
            if (bill is null)
                return Result<VendorPaymentReadDto>.Err("Vendor bill not found", "NOT_FOUND");

            // ----- (1) Prevent Overpayment -----
            var alreadyPaid = bill.VendorPayments
                .Where(p => p.Status == VendorPaymentStatus.Posted && p.Id != existing.Id)
                .Sum(p => p.Amount);

            if (dto.Amount + alreadyPaid > bill.Lines.Sum(l => l.LineTotal))
                return Result<VendorPaymentReadDto>.Err("Payment exceeds vendor bill balance", "VALIDATION_ERROR");

            // ----- (2) Handle Posting -----
            if (dto.Status == VendorPaymentStatus.Posted && existing.Status != VendorPaymentStatus.Posted)
            {
                // Auto-number
                if (string.IsNullOrEmpty(existing.PaymentNumber) || existing.PaymentNumber.StartsWith("Draft VP"))
                {
                    var nextNumber = await repo.GetNextSequenceValueAsync("vendor_payment_number_seq", tx);
                    existing.PaymentNumber = $"VP/{dto.PaymentDate.Year}/{nextNumber:000000}";
                }

                // Create Journal Entry (Dr AP / Cr Bank)
                var apAccount = await coaRepo.GetPayableAccountAsync(); // Accounts Payable
                var cashAccount = await coaRepo.GetCashOrBankAccountAsync(existing.Method);

                var je = await journalService.PostAsync(
                    $"Vendor Payment {existing.PaymentNumber} for Bill {bill.BillNumber}",
                    dto.PaymentDate,
                    new[]
                    {
                        (apAccount, dto.Amount, 0m),
                        (cashAccount, 0m, dto.Amount)
                    }
                );
                existing.JournalEntryId = je.Id;
            }

            // ----- (3) Handle Cancellation (reversal) -----
            if (dto.Status == VendorPaymentStatus.Cancelled && existing.Status == VendorPaymentStatus.Posted)
            {
                if (existing.JournalEntryId.HasValue)
                {
                    var je = await journalRepo.GetByIdAsync(existing.JournalEntryId.Value);
                    if (je != null) await journalService.ReverseAsync(je, "Vendor Payment cancelled");
                }
            }

            existing.PaymentDate = dto.PaymentDate;
            existing.Amount = dto.Amount;
            existing.Method = dto.Method;
            existing.Status = dto.Status;

            await repo.UpdateAsync(existing);
            await tx.CommitAsync();

            return await GetByIdAsync(existing.Id);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<VendorPaymentReadDto>.Err($"Failed to update payment: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<bool>.Err("Payment not found", "NOT_FOUND");
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
}