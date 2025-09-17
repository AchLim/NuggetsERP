using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("chart_of_account")]
public sealed class ChartOfAccount : BaseEntity
{
    [Required, MaxLength(32)]
    public string Code { get; set; } = string.Empty; // e.g. "1010"

    [Required, MaxLength(256)]
    public string Name { get; set; } = string.Empty; // e.g. "Cash", "Accounts Payable"

    [Required]
    public AccountType Type { get; set; }
}

public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}