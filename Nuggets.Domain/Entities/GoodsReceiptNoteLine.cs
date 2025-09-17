using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;


[Table("goods_receipt_note_line")]
public sealed class GoodsReceiptNoteLine : BaseEntity
{
    public Guid GoodsReceiptNoteId { get; set; }
    public GoodsReceiptNote GoodsReceiptNote { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid UomId { get; set; }
    public UnitOfMeasure Uom { get; set; } = null!;

    public decimal Quantity { get; set; }
}