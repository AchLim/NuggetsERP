using System.ComponentModel.DataAnnotations;

namespace Nuggets.Domain.Entities;

public sealed class ProductCategory : BaseEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; }

    public Guid? ParentId { get; set; }
    public ProductCategory? Parent { get; set; }
}
