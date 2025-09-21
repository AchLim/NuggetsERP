using Nuggets.Domain.Entities;

namespace Nuggets.Application.Common.Interfaces;

public interface IPurchaseOrderRepository : IGenericRepository<PurchaseOrder>
{
    Task<PurchaseOrder?> GetWithLinesAndGrnsAsync(Guid id, CancellationToken ct = default);
    Task<PurchaseOrder?> GetWithLinesAndBillsAsync(Guid id, CancellationToken ct = default);
}
public interface IPurchaseReceiptRepository : IGenericRepository<PurchaseReceipt> { }

public interface IVendorBillRepository : IGenericRepository<VendorBill>
{
    Task<VendorBill?> GetByIdWithLinesAsync(Guid id, CancellationToken ct = default);
}
public interface IVendorPaymentRepository : IGenericRepository<VendorPayment> { }