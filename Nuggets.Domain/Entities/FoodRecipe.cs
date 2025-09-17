using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("food_recipe")]
public class FoodRecipe : BaseEntity
{
    [Required]
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    [Required]
    public Guid FoodMaterialId { get; set; }
    public FoodMaterial FoodMaterial { get; set; } = null!;
    
    [Column(TypeName = "decimal(18,3)"), Range(0.00, 999999)]
    public decimal Quantity { get; set; }
    
    [Required]
    public Guid UomId { get; set; }
    public UnitOfMeasure Uom { get; set; } = null!;
}