using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("uom_conversion")]
public class UnitOfMeasureConversion : BaseEntity
{
    [Required]
    public Guid FromUomId { get; set; }
    public UnitOfMeasure FromUom { get; set; } = null!;
    
    [Required]
    public Guid ToUomId { get; set; }
    public UnitOfMeasure ToUom { get; set; } = null!;

    [Required, Column(TypeName = "decimal(18,6)"), Range(0.000001, 999999999)]  // allow very precise conversions
    public decimal ConversionRate { get; set; }

    // If true → you can use both directions (e.g., 1 Kg = 1000 g, 1000 g = 1 Kg automatically)
    public bool IsBidirectional { get; set; } = true;
}