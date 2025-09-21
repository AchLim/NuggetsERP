using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class CustomerPaymentService(
    ICustomerPaymentRepository repo,
    ICustomerInvoiceRepository invoiceRepo,
    IChartOfAccountRepository coaRepo,
    IJournalEntryRepository journalRepo,
    IJournalEntryService journalService
) : ICustomerPaymentService
{
    public async Task<Result<PagedResult<CustomerPaymentListDto>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, total) = await repo.GetPagedAsync(page, pageSize);
        var list = items.Select(ToListDto).ToList();
        return Result<PagedResult<CustomerPaymentListDto>>.Ok(new PagedResult<CustomerPaymentListDto>(list, total, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<CustomerPaymentListDto>>> GetAllAsync()
    {
        var items = await repo.GetAllAsync();
        return Result<IReadOnlyList<CustomerPaymentListDto>>.Ok(items.Select(ToListDto).ToList());
    }

    public async Task<Result<CustomerPaymentReadDto>> GetByIdAsync(Guid id)
    {
        var ent = await repo.GetByIdAsync(id);
        return ent is not null ? Result<CustomerPaymentReadDto>.Ok(ToReadDto(ent)) : Result<CustomerPaymentReadDto>.Err("Payment not found", "NOT_FOUND");
    }

    public async Task<Result<CustomerPaymentReadDto>> CreateAsync(CustomerPaymentCreateDto dto)
    {
        if (dto.Amount <= 0) return Result<CustomerPaymentReadDto>.Err("Amount must be positive", "VALIDATION_ERROR");
        
        var invoice = await invoiceRepo.GetByIdAsync(dto.CustomerInvoiceId);
        if (invoice is null) return Result<CustomerPaymentReadDto>.Err("Invoice not found", "NOT_FOUND");
        
        
        var paidSoFar = invoice.CustomerPayments
            .Where(p => p.Status == CustomerPaymentStatus.Posted)
            .Sum(p => p.Amount);

        if (paidSoFar + dto.Amount > invoice.Lines.Sum(l => l.LineTotal))
            return Result<CustomerPaymentReadDto>.Err("Payment exceeds invoice balance", "VALIDATION_ERROR");

        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var draftLabel = $"Draft CP *{DateTime.UtcNow:yyyyMMddHHmmss}";

            var ent = new CustomerPayment
            {
                CustomerInvoiceId = dto.CustomerInvoiceId,
                PaymentNumber = draftLabel,
                PaymentDate = dto.PaymentDate,
                Amount = dto.Amount,
                Method = dto.Method,
                Status = CustomerPaymentStatus.Draft
            };

            await repo.AddAsync(ent);

            await tx.CommitAsync();
            return Result<CustomerPaymentReadDto>.Ok(ToReadDto(ent));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<CustomerPaymentReadDto>.Err($"Failed to create payment: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<CustomerPaymentReadDto>> UpdateAsync(Guid id, CustomerPaymentUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<CustomerPaymentReadDto>.Err("Payment not found", "NOT_FOUND");

            var invoice = await invoiceRepo.GetByIdAsync(existing.CustomerInvoiceId);
            if (invoice is null) 
                return Result<CustomerPaymentReadDto>.Err("Invoice not found", "NOT_FOUND");

            // ===== 1. Validation: Prevent Overpayment =====
            var alreadyPaid = invoice.CustomerPayments
                .Where(p => p.Status == CustomerPaymentStatus.Posted && p.Id != existing.Id)
                .Sum(p => p.Amount);

            var invoiceTotal = invoice.Lines.Sum(l => l.LineTotal);

            if (dto.Amount + alreadyPaid > invoiceTotal)
                return Result<CustomerPaymentReadDto>.Err("Payment exceeds invoice balance", "VALIDATION_ERROR");

            // ===== 2. Handle Posting =====
            if (dto.Status == CustomerPaymentStatus.Posted && existing.Status != CustomerPaymentStatus.Posted)
            {
                // 1. Auto Generate Payment Number if payment still has a draft number
                if (string.IsNullOrEmpty(existing.PaymentNumber) || existing.PaymentNumber.StartsWith("Draft CP"))
                {
                    var nextNumber = await repo.GetNextSequenceValueAsync("customer_payment_number_seq");
                    existing.PaymentNumber = $"CP/{dto.PaymentDate.Year}/{nextNumber:000000}";
                }

                // 2. Create Journal Entry (Debit Cash/Bank, Credit AR)
                var cashAccount = await coaRepo.GetCashOrBankAccountAsync(dto.Method);
                var arAccount = await coaRepo.GetReceivableAccountAsync();

                var je = await journalService.PostAsync(
                    $"Customer Payment {existing.PaymentNumber} for Invoice {invoice.InvoiceNumber}",
                    dto.PaymentDate,
                    new[]
                    {
                        (cashAccount, dto.Amount, 0m),
                        (arAccount, 0m, dto.Amount)
                    });

                existing.JournalEntryId = je.Id;
            }
            
            // ===== 3. Handle Cancellation =====
            if (dto.Status == CustomerPaymentStatus.Cancelled && existing is { Status: CustomerPaymentStatus.Posted, JournalEntryId: not null })
            {
                if (existing.JournalEntryId.HasValue)
                {
                    var je = await journalRepo.GetByIdAsync(existing.JournalEntryId.Value);
                    if (je != null) await journalService.ReverseAsync(je, "Customer payment cancelled");
                }
            }
            
            existing.PaymentDate = dto.PaymentDate;
            existing.Amount = dto.Amount;
            existing.Method = dto.Method;
            existing.Status = dto.Status;

            await repo.UpdateAsync(existing);
            await tx.CommitAsync();
            return Result<CustomerPaymentReadDto>.Ok(ToReadDto(existing));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<CustomerPaymentReadDto>.Err($"Failed to update payment: {ex.Message}", "DB_ERROR");
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
            return Result<bool>.Err($"Failed to delete payment: {ex.Message}", "DB_ERROR");
        }
    }

    public CustomerPaymentListDto ToListDto(CustomerPayment p) => new CustomerPaymentListDto(p.Id,
        p.CustomerInvoiceId, p.CustomerInvoice.InvoiceNumber, p.CustomerInvoice.CustomerId,
        p.CustomerInvoice.Customer.Name, p.PaymentDate, p.Amount, p.Status, p.Method, p.PaymentNumber);
    

    public CustomerPaymentReadDto ToReadDto(CustomerPayment p) => new CustomerPaymentReadDto(p.Id,
        p.CustomerInvoiceId, p.CustomerInvoice.InvoiceNumber, p.CustomerInvoice.CustomerId,
        p.CustomerInvoice.Customer.Name, p.PaymentDate, p.Amount, p.Status, p.Method, p.PaymentNumber);
}
