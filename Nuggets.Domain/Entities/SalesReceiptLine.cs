using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("sales_receipt_line")]
public sealed class SalesReceiptLine : BaseEntity
{
    public Guid SalesReceiptId { get; set; }
    public SalesReceipt SalesReceipt { get; set; } = null!;

    [Required]
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    [Required]
    public Guid UomId { get; set; }
    public UnitOfMeasure Uom { get; set; } = null!;

    [Column(TypeName = "decimal(18,3)"), Range(0.00, 999999)]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal DiscountPercent { get; set; } = 0m;

    [NotMapped]
    public decimal LineTotal => Quantity * UnitPrice * (1 - DiscountPercent / 100m);
}
