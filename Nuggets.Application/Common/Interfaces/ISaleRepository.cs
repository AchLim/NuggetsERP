using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface ISalesOrderRepository : IGenericRepository<SalesOrder> { }
public interface ICustomerInvoiceRepository : IGenericRepository<CustomerInvoice> { }
public interface ISalesReceiptRepository : IGenericRepository<SalesReceipt> { }

public interface ICustomerPaymentRepository : IGenericRepository<CustomerPayment> { }