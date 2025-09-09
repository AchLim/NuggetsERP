using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

public class Sale : BaseEntity
{
    [Required]
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;
    
    [Range(0, 999999)]
    public float Quantity { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 9999999)]
    public decimal PricePerUnit { get; set; }

    [Required]
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
}