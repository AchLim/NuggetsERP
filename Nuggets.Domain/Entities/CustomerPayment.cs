using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("customer_payment")]
public sealed class CustomerPayment : BaseEntity
{
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [MaxLength(100)] public string PaymentNumber { get; set; } = string.Empty;

    public CustomerPaymentStatus Status { get; set; } = CustomerPaymentStatus.Draft;
    public CustomerPaymentMethod Method { get; set; } = CustomerPaymentMethod.Cash;

    // Relationships
    public Guid CustomerInvoiceId { get; set; }
    public CustomerInvoice CustomerInvoice { get; set; } = null!;
    
    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
}

public enum CustomerPaymentMethod
{
    Cash = 1,
    Bank = 2,
    Qris = 3,
    DebitCard = 4,
    CreditCard = 5,
    Other = 999
}

public enum CustomerPaymentStatus
{
    Draft = 1,
    Posted = 2,
    Cancelled = 9
}