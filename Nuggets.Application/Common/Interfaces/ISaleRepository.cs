using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface ISalesOrderRepository : IGenericRepository<SalesOrder>
{
    Task<SalesOrder?> GetWithLinesAndDnsAsync(Guid id, CancellationToken ct = default);
    Task<SalesOrder?> GetWithLinesAndInvoicesAsync(Guid id, CancellationToken ct = default);
}

public interface ICustomerInvoiceRepository : IGenericRepository<CustomerInvoice>
{
    Task<CustomerInvoice?> GetByIdWithLinesAsync(Guid id, CancellationToken ct = default);
}
public interface ISalesReceiptRepository : IGenericRepository<SalesReceipt> { }

public interface ICustomerPaymentRepository : IGenericRepository<CustomerPayment> { }