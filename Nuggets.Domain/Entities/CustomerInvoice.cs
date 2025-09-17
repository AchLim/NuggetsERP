using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

/// <summary>
/// CustomerInvoice = B2B sales (on credit).
/// Creates Accounts Receivable (AR) that must be cleared with CustomerPayment.
/// </summary>
[Table("customer_invoice")]
public sealed class CustomerInvoice : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public Guid? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    [Required, MaxLength(100)] public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);

    public CustomerInvoiceStatus Status { get; set; } = CustomerInvoiceStatus.Draft;

    public ICollection<CustomerInvoiceLine> Lines { get; set; } = new List<CustomerInvoiceLine>();
    public ICollection<CustomerPayment> CustomerPayments { get; set; } = new List<CustomerPayment>();
    
    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
}

public enum CustomerInvoiceStatus
{
    Draft = 1,
    Posted = 2,
    Paid = 3,
    Cancelled = 9
}