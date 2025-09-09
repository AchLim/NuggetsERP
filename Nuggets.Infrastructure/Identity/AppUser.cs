using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Nuggets.Domain.Entities;

namespace Nuggets.Infrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    public bool Active { get; set; } = true;
    
    public override Guid Id { get; set; } = Guid.CreateVersion7();
    
    [MaxLength(100)]
    public string? FullName { get; set; }
    
    [MaxLength(200)]
    public string? Address { get; set; }
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
}
