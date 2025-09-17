using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public sealed class ChartOfAccountRepository(NuggetsDbContext db)
    : GenericRepository<ChartOfAccount>(db), IChartOfAccountRepository
{
    public override async Task<(IReadOnlyList<ChartOfAccount> Items, int TotalCount)> GetPagedAsync(int page,
        int pageSize, IQueryable<ChartOfAccount>? startingQuery = null, CancellationToken ct = default)
    {
        var query = startingQuery ?? db.ChartOfAccounts.AsNoTracking().AsQueryable();

        query = query.OrderBy(coa => coa.Code);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, totalCount);
    }

    // -------------------- CASH & BANK --------------------

    public async Task<ChartOfAccount> GetCashOrBankAccountAsync(CustomerPaymentMethod method,
        CancellationToken ct = default)
    {
        var accountCode = method switch
        {
            CustomerPaymentMethod.Cash       => "1000", // Cash
            CustomerPaymentMethod.Bank       => "1010", // Bank-BCA (default)
            CustomerPaymentMethod.DebitCard  => "1010",
            CustomerPaymentMethod.CreditCard => "1010",
            CustomerPaymentMethod.Qris       => "1020", // QRIS Receivable
            CustomerPaymentMethod.Other      => "1000",
            _ => throw new InvalidOperationException($"No Chart of Account mapping defined for payment method {method}")
        };

        return await FindByCodeAsync(accountCode, "Cash/Bank Account", ct);
    }

    public async Task<ChartOfAccount> GetCashOrBankAccountAsync(VendorPaymentMethod method,
        CancellationToken ct = default)
    {
        var accountCode = method switch
        {
            VendorPaymentMethod.Cash       => "1000",
            VendorPaymentMethod.Bank       => "1010",
            VendorPaymentMethod.DebitCard  => "1010",
            VendorPaymentMethod.CreditCard => "1010",
            VendorPaymentMethod.Qris       => "1020",
            VendorPaymentMethod.Other      => "1000",
            _ => throw new InvalidOperationException($"No Chart of Account mapping defined for payment method {method}")
        };

        return await FindByCodeAsync(accountCode, "Cash/Bank Account", ct);
    }

    public async Task<ChartOfAccount> GetCashOrBankAccountAsync(PurchaseReceiptPaymentMethod method,
        CancellationToken ct = default)
    {
        var accountCode = method switch
        {
            PurchaseReceiptPaymentMethod.Cash       => "1000",
            PurchaseReceiptPaymentMethod.Bank       => "1010",
            PurchaseReceiptPaymentMethod.DebitCard  => "1010",
            PurchaseReceiptPaymentMethod.CreditCard => "1010",
            PurchaseReceiptPaymentMethod.Qris       => "1020",
            PurchaseReceiptPaymentMethod.Other      => "1000",
            _ => throw new InvalidOperationException($"No Chart of Account mapping defined for payment method {method}")
        };

        return await FindByCodeAsync(accountCode, "Cash/Bank Account", ct);
    }
    

    public async Task<ChartOfAccount> GetCashOrBankAccountAsync(SalesReceiptPaymentMethod method,
        CancellationToken ct = default)
    {
        var accountCode = method switch
        {
            SalesReceiptPaymentMethod.Cash       => "1000",
            SalesReceiptPaymentMethod.Bank       => "1010",
            SalesReceiptPaymentMethod.DebitCard  => "1010",
            SalesReceiptPaymentMethod.CreditCard => "1010",
            SalesReceiptPaymentMethod.Qris       => "1020",
            SalesReceiptPaymentMethod.Other      => "1000",
            _ => throw new InvalidOperationException($"No Chart of Account mapping defined for payment method {method}")
        };

        return await FindByCodeAsync(accountCode, "Cash/Bank Account", ct);
    }

    // -------------------- BUSINESS CRITICAL ACCOUNTS --------------------
    public async Task<ChartOfAccount> GetInventoryAccountAsync(CancellationToken ct = default) =>
        await FindByCodeAsync("1050", "Inventory", ct);

    public async Task<ChartOfAccount> GetPayableAccountAsync(CancellationToken ct = default) =>
        await FindByCodeAsync("2000", "Accounts Payable", ct);

    public async Task<ChartOfAccount> GetReceivableAccountAsync(CancellationToken ct = default) =>
        await FindByCodeAsync("1030", "Accounts Receivable", ct);

    public async Task<ChartOfAccount> GetRevenueAccountAsync(CancellationToken ct = default) =>
        await FindByCodeAsync("4000", "Sales Revenue", ct);

    public async Task<ChartOfAccount> GetCogsAccountAsync(CancellationToken ct = default) =>
        await FindByCodeAsync("5000", "Cost of Goods Sold", ct);

    // -------------------- TAX ACCOUNTS --------------------

    public async Task<ChartOfAccount> GetVatInputAccountAsync(CancellationToken ct = default) =>
        await FindByCodeAsync("1040", "VAT Input (PPN Masukan)", ct);

    public async Task<ChartOfAccount> GetVatOutputAccountAsync(CancellationToken ct = default) =>
        await FindByCodeAsync("2100", "VAT Output (PPN Keluaran)", ct);

    public async Task<ChartOfAccount> GetExciseDutyAccountAsync(CancellationToken ct = default) =>
        await FindByCodeAsync("2110", "Excise Duty Payable (Cukai)", ct);

    // -------------------- GRNI CLEARING --------------------

    public async Task<ChartOfAccount> GetGrniAccountAsync(CancellationToken ct = default) =>
        await FindByCodeAsync("2200", "Goods Received Not Invoiced (GRNI)", ct);

    // -------------------- HELPER --------------------
    private async Task<ChartOfAccount> FindByCodeAsync(
        string code, string friendlyName, CancellationToken ct = default)
    {
        var acct = await db.ChartOfAccounts.FirstOrDefaultAsync(a => a.Code == code, ct);
        if (acct == null)
            throw new InvalidOperationException($"{friendlyName} account not found (Code {code})!");
        return acct;
    }
}