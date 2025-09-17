using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("delivery_note_line")]
public sealed class DeliveryNoteLine : BaseEntity
{
    public Guid DeliveryNoteId { get; set; }
    public DeliveryNote DeliveryNote { get; set; } = null!;

    [Required]
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required]
    public Guid UomId { get; set; }
    public UnitOfMeasure Uom { get; set; } = null!;

    public decimal Quantity { get; set; }
}