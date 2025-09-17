using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("sales_receipt")]
public sealed class SalesReceipt : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    [MaxLength(100)] public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

    public SalesReceiptStatus Status { get; set; } = SalesReceiptStatus.Draft;
    public SalesReceiptPaymentMethod Method { get; set; } = SalesReceiptPaymentMethod.Cash;

    public ICollection<SalesReceiptLine> Lines { get; set; } = new List<SalesReceiptLine>();
    public ICollection<CustomerPayment> Payments { get; set; } = new List<CustomerPayment>();
    
    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
}

public enum SalesReceiptPaymentMethod
{
    Cash = 1,
    Bank = 2,
    Qris = 3,
    DebitCard = 4,
    CreditCard = 5,
    Other = 999
}


public enum SalesReceiptStatus
{
    Draft = 0,
    Posted = 1,
    Cancelled = 9
}