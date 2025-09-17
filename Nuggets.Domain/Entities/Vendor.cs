using Nuggets.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("vendor")]
public sealed class Vendor : BaseEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}
