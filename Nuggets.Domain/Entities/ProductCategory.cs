using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("product_category")]
public sealed class ProductCategory : BaseEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; }

    public Guid? ParentId { get; set; }
    public ProductCategory? Parent { get; set; }
}
