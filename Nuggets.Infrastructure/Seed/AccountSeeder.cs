using Nuggets.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Seed;

public static class AccountSeeder
{
    public static async Task SeedChartOfAccountsAsync(NuggetsDbContext db)
    {
        if (await db.ChartOfAccounts.AnyAsync()) return;

        var accounts = new List<ChartOfAccount>
        {
            // ------------------------------
            // Assets (Harta) 1000–1999
            // ------------------------------
            new ChartOfAccount { Code = "1000", Name = "Cash on Hand (Kas)", Type = AccountType.Asset },
            new ChartOfAccount { Code = "1010", Name = "Bank", Type = AccountType.Asset },
            new ChartOfAccount { Code = "1020", Name = "QRIS Receivable", Type = AccountType.Asset },
            new ChartOfAccount { Code = "1030", Name = "Accounts Receivable", Type = AccountType.Asset },

            new ChartOfAccount { Code = "1040", Name = "VAT Input", Type = AccountType.Asset },
            new ChartOfAccount { Code = "1050", Name = "Inventory - Vape & Accessories", Type = AccountType.Asset },
            new ChartOfAccount { Code = "1060", Name = "Prepaid Expenses", Type = AccountType.Asset },

            // Fixed Assets
            new ChartOfAccount { Code = "1500", Name = "Fixed Assets - Equipment/Furniture", Type = AccountType.Asset },
            new ChartOfAccount { Code = "1510", Name = "Accumulated Depreciation", Type = AccountType.Asset },

            // ------------------------------
            // Liabilities (Kewajiban) 2000–2999
            // ------------------------------
            new ChartOfAccount { Code = "2000", Name = "Accounts Payable", Type = AccountType.Liability },
            new ChartOfAccount { Code = "2100", Name = "VAT Output", Type = AccountType.Liability },
            new ChartOfAccount { Code = "2110", Name = "Excise Duty Payable", Type = AccountType.Liability },
            new ChartOfAccount { Code = "2200", Name = "Goods Received Not Invoiced", Type = AccountType.Liability },
            new ChartOfAccount { Code = "2300", Name = "Accrued Expenses", Type = AccountType.Liability },
            new ChartOfAccount { Code = "2500", Name = "Business Loan Payable", Type = AccountType.Liability },

            // ------------------------------
            // Equity (Ekuitas) 3000–3999
            // ------------------------------
            new ChartOfAccount { Code = "3000", Name = "Owner’s Equity", Type = AccountType.Equity },
            new ChartOfAccount { Code = "3100", Name = "Retained Earnings", Type = AccountType.Equity },
            new ChartOfAccount { Code = "3200", Name = "Current Year Profit", Type = AccountType.Equity },

            // ------------------------------
            // Revenue (Pendapatan) 4000–4999
            // ------------------------------
            new ChartOfAccount { Code = "4000", Name = "Sales Revenue", Type = AccountType.Revenue },
            new ChartOfAccount { Code = "4010", Name = "Sales Returns & Discounts", Type = AccountType.Revenue },
            new ChartOfAccount { Code = "4100", Name = "Other Income", Type = AccountType.Revenue },

            // ------------------------------
            // COGS (Harga Pokok Penjualan) 5000–5099
            // ------------------------------
            new ChartOfAccount { Code = "5000", Name = "COGS - Vape & Accessories", Type = AccountType.Expense },

            // ------------------------------
            // Expenses (Beban Operasional) 5100–5999
            // ------------------------------
            new ChartOfAccount { Code = "5100", Name = "Salaries & Wages (Gaji)", Type = AccountType.Expense },
            new ChartOfAccount { Code = "5110", Name = "Rent Expense (Sewa)", Type = AccountType.Expense },
            new ChartOfAccount { Code = "5120", Name = "Utilities Expense (Listrik, Air, Internet)", Type = AccountType.Expense },
            new ChartOfAccount { Code = "5130", Name = "Marketing & Promotion Expense", Type = AccountType.Expense },
            new ChartOfAccount { Code = "5140", Name = "Transportation Expense", Type = AccountType.Expense },
            new ChartOfAccount { Code = "5150", Name = "Office Supplies Expense", Type = AccountType.Expense },
            new ChartOfAccount { Code = "5160", Name = "Depreciation Expense", Type = AccountType.Expense },
            new ChartOfAccount { Code = "5170", Name = "Other General Expenses", Type = AccountType.Expense }
        };

        await db.ChartOfAccounts.AddRangeAsync(accounts);
        await db.SaveChangesAsync();
    }
}