using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface IPurchaseOrderRepository : IGenericRepository<PurchaseOrder> { }
public interface IPurchaseReceiptRepository : IGenericRepository<PurchaseReceipt> { }
public interface IVendorBillRepository : IGenericRepository<VendorBill> { }
public interface IVendorPaymentRepository : IGenericRepository<VendorPayment> { }