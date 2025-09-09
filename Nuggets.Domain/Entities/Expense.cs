using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

public enum ExpenseCategory
{
    FoodMaterial = 1,
    Labor = 2,
    Electricity = 3,
    Wifi = 4,
    Overhead = 5,
    Other = 999
}

public class Expense : BaseEntity
{
    [Required]
    public ExpenseCategory Category { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = default!;
    
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 9999999)]
    public decimal Amount { get; set; }

    [Required] public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
}