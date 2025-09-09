using Nuggets.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Nuggets.Domain.Entities;

public sealed class Customer : BaseEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}
