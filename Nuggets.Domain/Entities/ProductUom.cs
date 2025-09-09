using System.ComponentModel.DataAnnotations;

namespace Nuggets.Domain.Entities;

// FIXME: Probably unused, to remove. Replaced by UnitOfMeasure
public sealed class ProductUom : BaseEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public decimal Ratio { get; set; }
    public decimal Rounding { get; set; }
}
