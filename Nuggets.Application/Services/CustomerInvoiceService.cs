using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class CustomerInvoiceService(
    ICustomerInvoiceRepository repo,
    IJournalEntryRepository journalRepo,
    IChartOfAccountRepository coaRepo,
    IJournalEntryService journalService
) : ICustomerInvoiceService
{
    public async Task<Result<PagedResult<CustomerInvoiceListDto>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, total) = await repo.GetPagedAsync(page, pageSize);
        var list = items.Select(ToListDto).ToList();
        return Result<PagedResult<CustomerInvoiceListDto>>.Ok(new PagedResult<CustomerInvoiceListDto>(list, total, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<CustomerInvoiceListDto>>> GetAllAsync()
    {
        var items = await repo.GetAllAsync();
        return Result<IReadOnlyList<CustomerInvoiceListDto>>.Ok(items.Select(ToListDto).ToList());
    }

    public async Task<Result<CustomerInvoiceReadDto>> GetByIdAsync(Guid id)
    {
        var e = await repo.GetByIdAsync(id);
        return e is not null ? Result<CustomerInvoiceReadDto>.Ok(ToReadDto(e)) : Result<CustomerInvoiceReadDto>.Err("Invoice not found", "NOT_FOUND");
    }

    public async Task<Result<CustomerInvoiceReadDto>> CreateAsync(CustomerInvoiceCreateDto dto)
    {
        if (dto.Lines.Count == 0) return Result<CustomerInvoiceReadDto>.Err("At least one line required", "VALIDATION_ERROR");
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var draftLabel = $"Draft CI *{DateTime.UtcNow:yyyyMMddHHmmss}";

            var ent = new CustomerInvoice
            {
                CustomerId = dto.CustomerId,
                SalesOrderId = dto.SalesOrderId,
                InvoiceNumber = draftLabel,
                InvoiceDate = dto.InvoiceDate,
                DueDate = dto.DueDate,
                Status = CustomerInvoiceStatus.Draft,
                Lines = dto.Lines.Select(l => new CustomerInvoiceLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice
                }).ToList()
            };

            await repo.AddAsync(ent);
            await tx.CommitAsync();
            return Result<CustomerInvoiceReadDto>.Ok(ToReadDto(ent));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<CustomerInvoiceReadDto>.Err($"Failed to create invoice: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<CustomerInvoiceReadDto>> UpdateAsync(Guid id, CustomerInvoiceUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<CustomerInvoiceReadDto>.Err("Invoice not found", "NOT_FOUND");

            
            if (dto.Status == CustomerInvoiceStatus.Posted && 
                (string.IsNullOrEmpty(existing.InvoiceNumber) || existing.InvoiceNumber.StartsWith("Draft CI")))
            {
                // Generate auto number
                var nextNumber = await repo.GetNextSequenceValueAsync("customer_invoice_number_seq");
                existing.InvoiceNumber = $"CI/{dto.InvoiceDate.Year}/{nextNumber:000000}";
            }
            
            // --- Handle Posting ---
            if (dto.Status == CustomerInvoiceStatus.Posted && existing.Status != CustomerInvoiceStatus.Posted)
            {
                var arAcc   = await coaRepo.GetReceivableAccountAsync();
                var revAcc  = await coaRepo.GetRevenueAccountAsync();
                var cogsAcc = await coaRepo.GetCogsAccountAsync();
                var invAcc  = await coaRepo.GetInventoryAccountAsync();

                var revenueTotal = existing.Lines.Sum(l => l.Quantity * l.UnitPrice);

                var cogsTotal = existing.Lines.Sum(l => (l.Product?.CurrentMovingAverageCost ?? 0) * l.Quantity);

                var je = await journalService.PostAsync(
                    $"Customer Invoice {existing.InvoiceNumber}",
                    dto.InvoiceDate,
                    new[]
                    {
                        (arAcc,   revenueTotal, 0m), // Dr Accounts Receivable
                        (revAcc,  0m, revenueTotal), // Cr Revenue
                        (cogsAcc, cogsTotal, 0m),    // Dr COGS
                        (invAcc,  0m, cogsTotal)     // Cr Inventory
                    });

                existing.JournalEntryId = je.Id;
            }
            // --- Handle Cancellation ---
            else if (dto.Status == CustomerInvoiceStatus.Cancelled && existing.Status == CustomerInvoiceStatus.Posted)
            {
                if (existing.JournalEntryId.HasValue)
                {
                    var je = await journalRepo.GetByIdAsync(existing.JournalEntryId.Value);
                    if (je != null) await journalService.ReverseAsync(je, "Customer Invoice cancelled");
                }
            }
            // Paid status: usually marked by linked CustomerPayment(s),
            // so here we just reflect the new status.
            else if (dto.Status == CustomerInvoiceStatus.Paid && existing.Status == CustomerInvoiceStatus.Posted)
            {
                // nothing new to post, CustomerPaymentService handles accounting
            }
            
            existing.CustomerId = dto.CustomerId;
            existing.SalesOrderId = dto.SalesOrderId;
            existing.InvoiceDate = dto.InvoiceDate;
            existing.DueDate = dto.DueDate;
            existing.Status = dto.Status;

            existing.Lines.Clear();
            foreach (var l in dto.Lines)
            {
                existing.Lines.Add(new CustomerInvoiceLine
                {
                    ProductId = l.ProductId,
                    UomId = l.UomId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice
                });
            }

            await repo.UpdateAsync(existing);
            await tx.CommitAsync();
            return Result<CustomerInvoiceReadDto>.Ok(ToReadDto(existing));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<CustomerInvoiceReadDto>.Err($"Failed to update invoice: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return Result<bool>.Err("Invoice not found", "NOT_FOUND");
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

    private static CustomerInvoiceListDto ToListDto(CustomerInvoice i) => new(i.Id, i.SalesOrderId, i.SalesOrder?.OrderNumber, i.CustomerId,
        i.Customer?.Name, i.InvoiceNumber, i.InvoiceDate, i.DueDate, i.Status.ToString(), i.Lines.Sum(l => l.Quantity * l.UnitPrice));
    
    private static CustomerInvoiceReadDto ToReadDto(CustomerInvoice i) =>
        new(i.Id, i.SalesOrderId, i.SalesOrder?.OrderNumber, i.CustomerId, i.Customer?.Name, i.InvoiceNumber, i.InvoiceDate, i.DueDate, i.Status,
            i.Lines.Select(l => new CustomerInvoiceLineReadDto(l.Id, l.ProductId, l.Product?.Name, l.UomId, l.Quantity, l.UnitPrice, l.Quantity * l.UnitPrice)).ToList());
}
