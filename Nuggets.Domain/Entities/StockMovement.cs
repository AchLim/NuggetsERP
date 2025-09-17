using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("stock_movement")]
public sealed class StockMovement : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public StockMovementType MovementType { get; set; }

    /// <summary>Always positive. Direction decided by MovementType.</summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; } // Cost per unit for Inbound (from Purchase)
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;

    /// <summary>Optional link to Purchase or Sale Document</summary>
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; } // e.g. "Purchase", "Sale"
}

public enum StockMovementType
{
    Inbound = 1,
    Outbound = 2,
    Adjustment = 3
}