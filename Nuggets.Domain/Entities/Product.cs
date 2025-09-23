using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("product")]
public sealed class Product : BaseEntity
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Default Purchase Price (used as hint when creating PO/vendor bill).
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DefaultPurchasePrice { get; set; }

    /// <summary>
    /// Default Sales Price (used as list price in SO/Invoices when no Pricelist rule found).
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DefaultSalesPrice { get; set; }

    /// <summary>
    /// Moving Average Cost for stock valuation. This changes as GRNs/PRs are received.
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal CurrentMovingAverageCost { get; set; } = 0m;
    
    /// <summary>
    /// Base unit of measure (e.g. Bottle, Piece, Pack).
    /// </summary>
    [Required]
    public Guid UomId { get; set; }
    public UnitOfMeasure Uom { get; set; } = null!;

    /// <summary>
    /// Optional category (e.g. Juice, Coil, Device).
    /// </summary>
    public Guid? ProductCategoryId { get; set; }
    public ProductCategory? ProductCategory { get; set; }

    /// <summary>
    /// Preferred supplier (optional).
    /// </summary>
    public Guid? VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    
    
    public CostMethod CostMethod { get; set; } = CostMethod.MovingAverage;

    /// <summary>
    /// All stock movements related to this product.
    /// </summary>
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    
    public ICollection<FoodRecipe> FoodRecipes { get; set; } = new List<FoodRecipe>();
}

public enum CostMethod
{
    Standard,
    MovingAverage,
    FIFO
}
