using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("payment")]
public sealed class VendorPayment : BaseEntity
{
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [MaxLength(100)] public string PaymentNumber { get; set; } = string.Empty;

    public VendorPaymentStatus Status { get; set; } = VendorPaymentStatus.Draft;
    public VendorPaymentMethod Method { get; set; } = VendorPaymentMethod.Cash;

    [Required]
    public Guid VendorBillId { get; set; }
    public VendorBill VendorBill { get; set; } = null!;
    
    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
}

public enum VendorPaymentMethod
{
    Cash = 1,
    Bank = 2,
    Qris = 3,
    DebitCard = 4,
    CreditCard = 5,
    Other = 999
}

public enum VendorPaymentStatus
{
    Draft = 1,
    Posted = 2,
    Cancelled = 9
}

