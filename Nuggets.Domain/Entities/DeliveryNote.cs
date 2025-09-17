using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("delivery_note")]
public sealed class DeliveryNote : BaseEntity
{
    public Guid SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = null!;

    [MaxLength(100)] 
    public string DeliveryNumber { get; set; } = string.Empty;

    public DateTime DeliveryDate { get; set; } = DateTime.UtcNow;

    public DeliveryNoteStatus Status { get; set; } = DeliveryNoteStatus.Draft;

    public ICollection<DeliveryNoteLine> Lines { get; set; } = new List<DeliveryNoteLine>();

    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
}

public enum DeliveryNoteStatus
{
    Draft = 1,
    Delivered = 2,
    Cancelled = 9
}