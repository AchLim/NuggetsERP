using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("uom_uom")]
public class UnitOfMeasure : BaseEntity
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required, MaxLength(10)]
    public string Abbreviation { get; set; } = string.Empty;

    public ICollection<UnitOfMeasureConversion> FromConversions { get; set; } = new List<UnitOfMeasureConversion>();
    public ICollection<UnitOfMeasureConversion> ToConversions { get; set; } = new List<UnitOfMeasureConversion>();
}