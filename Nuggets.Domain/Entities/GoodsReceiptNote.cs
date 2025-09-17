using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Nuggets.Domain.Entities;

[Table("goods_receipt_note")]
public sealed class GoodsReceiptNote : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    [MaxLength(100)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public string GRNNumber { get; set; } = string.Empty;

    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

    public GoodsReceiptNoteStatus Status { get; set; } = GoodsReceiptNoteStatus.Draft;

    public ICollection<GoodsReceiptNoteLine> Lines { get; set; } = new List<GoodsReceiptNoteLine>();

    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
}

public enum GoodsReceiptNoteStatus
{
    Draft = 1,
    Received = 2,
    Cancelled = 9
}