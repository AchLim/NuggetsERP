using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;


/// <summary>
/// PurchaseReceipt = POS purchase (paid upfront).
/// You already paid when goods were received,
/// so no AP will be created. JE: Dr Inventory / Cr Cash.
/// </summary>
[Table("purchase_receipt")]
public sealed class PurchaseReceipt : BaseEntity
{
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;

    [MaxLength(100)] public string ReceiptNumber { get; set; } = string.Empty;

    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

    public PurchaseReceiptStatus Status { get; set; } = PurchaseReceiptStatus.Draft;

    public PurchaseReceiptPaymentMethod Method { get; set; } = PurchaseReceiptPaymentMethod.Cash;

    public ICollection<PurchaseReceiptLine> Lines { get; set; } = new List<PurchaseReceiptLine>();
                                
    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
}

public enum PurchaseReceiptStatus
{
    Draft = 1,
    Received = 2,
    Cancelled = 9
}

public enum PurchaseReceiptPaymentMethod
{
    Cash = 1,
    Bank = 2,
    Qris = 3,
    DebitCard = 4,
    CreditCard = 5,
    Other = 999
}