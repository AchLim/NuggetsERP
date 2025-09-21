using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("sales_order")]
public sealed class SalesOrder : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    [MaxLength(100)] public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;

    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
    public ICollection<DeliveryNote> DeliveryNotes { get; set; } = new List<DeliveryNote>();
    public ICollection<CustomerInvoice> CustomerInvoices { get; set; } = new List<CustomerInvoice>();
}

public enum SalesOrderStatus
{
    Draft = 1,
    Confirmed = 2,
    Delivered = 3,
    Cancelled = 9
}