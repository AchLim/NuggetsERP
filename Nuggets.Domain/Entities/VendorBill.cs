using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("vendor_bill")]
public sealed class VendorBill : BaseEntity
{
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;

    public Guid? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }


    [MaxLength(100)] public string BillNumber { get; set; } = string.Empty;
    public DateTime BillDate { get; set; } = DateTime.UtcNow;

    public VendorBillStatus Status { get; set; } = VendorBillStatus.Draft;

    public ICollection<VendorBillLine> Lines { get; set; } = new List<VendorBillLine>();
    public ICollection<VendorPayment> VendorPayments { get; set; } = new List<VendorPayment>();
    
    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
}

public enum VendorBillStatus
{
    Draft = 1,
    Posted = 2,
    Paid = 3,
    Cancelled = 9
}