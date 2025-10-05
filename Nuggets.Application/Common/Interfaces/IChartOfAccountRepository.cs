using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface IChartOfAccountRepository : IGenericRepository<ChartOfAccount>
{
    // Cash/Bank Accounts
    Task<ChartOfAccount> GetCashOrBankAccountAsync(CustomerPaymentMethod method, CancellationToken ct = default);
    Task<ChartOfAccount> GetCashOrBankAccountAsync(VendorPaymentMethod method, CancellationToken ct = default);
    Task<ChartOfAccount> GetCashOrBankAccountAsync(PurchaseReceiptPaymentMethod method, CancellationToken ct = default);
    Task<ChartOfAccount> GetCashOrBankAccountAsync(SalesReceiptPaymentMethod method, CancellationToken ct = default);

    // Core Accounts
    Task<ChartOfAccount> GetInventoryAccountAsync(CancellationToken ct = default);
    Task<ChartOfAccount> GetPayableAccountAsync(CancellationToken ct = default);
    Task<ChartOfAccount> GetReceivableAccountAsync(CancellationToken ct = default);
    Task<ChartOfAccount> GetRevenueAccountAsync(CancellationToken ct = default);
    Task<ChartOfAccount> GetCogsAccountAsync(CancellationToken ct = default);

    // Tax Accounts
    Task<ChartOfAccount> GetVatInputAccountAsync(CancellationToken ct = default);
    Task<ChartOfAccount> GetVatOutputAccountAsync(CancellationToken ct = default);
    Task<ChartOfAccount> GetExciseDutyAccountAsync(CancellationToken ct = default);

    // Clearing Accounts
    Task<ChartOfAccount> GetGrniAccountAsync(CancellationToken ct = default);

    Task<ChartOfAccount> GetInventoryAdjustmentAccountAsync(CancellationToken ct = default);
}