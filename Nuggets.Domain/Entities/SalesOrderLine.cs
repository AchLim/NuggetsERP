using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("sales_order_line")]
public sealed class SalesOrderLine : BaseEntity
{
    public Guid SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = null!;

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
}