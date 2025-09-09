using Microsoft.AspNetCore.Identity;

namespace Nuggets.Infrastructure.Identity;

public class AppRole : IdentityRole<Guid>
{
    public override Guid Id { get; set; } = Guid.CreateVersion7();
}
