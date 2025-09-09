namespace Nuggets.Domain.Entities;

public abstract class BaseEntity
{
    public bool Active { get; set; } = true;
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; } = false;
}
