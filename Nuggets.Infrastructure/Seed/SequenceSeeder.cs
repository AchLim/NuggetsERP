using Microsoft.EntityFrameworkCore;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Seed;

public static class SequenceSeeder
{
    public static async Task EnsureSequencesAsync(NuggetsDbContext dbContext)
    {
        var sequences = new Dictionary<string, long>
        {
            // Accounting
            ["journal_entry_number_seq"] = 1,
            
            // Stock
            ["grn_number_seq"] = 1,
            ["dn_number_seq"] = 1,
            
            // Sales
            ["sales_order_number_seq"] = 1,
            ["sales_receipt_number_seq"] = 1,
            ["customer_invoice_number_seq"] = 1,
            ["customer_payment_number_seq"] = 1,

            // Purchase
            ["purchase_order_number_seq"] = 1,
            ["purchase_receipt_number_seq"] = 1,
            ["vendor_bill_number_seq"] = 1,
            ["vendor_payment_number_seq"] = 1
        };

        foreach (var seq in sequences)
        {
            var sql = $"CREATE SEQUENCE IF NOT EXISTS \"{seq.Key}\" START {seq.Value} INCREMENT 1;";
            await dbContext.Database.ExecuteSqlRawAsync(sql);
        }
    }
}