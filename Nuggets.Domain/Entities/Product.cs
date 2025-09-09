using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

public sealed class Product : BaseEntity
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 9999999)]
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public ProductCategory? ProductCategory { get; set; }
    public Guid? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    

    public ICollection<FoodRecipe> FoodRecipes { get; set; } = new List<FoodRecipe>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
