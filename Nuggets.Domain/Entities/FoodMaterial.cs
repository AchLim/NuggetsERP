using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("food_material")]
public class FoodMaterial : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    [Column(TypeName = "decimal(18,2)"), Range(0.00, 9999999)]
    public decimal UnitPrice { get; set; }
    
    [Required]
    public Guid UomId { get; set; }
    public UnitOfMeasure Uom { get; set; } = null!;

    public ICollection<FoodRecipe> FoodRecipes { get; set; } = new List<FoodRecipe>();
}