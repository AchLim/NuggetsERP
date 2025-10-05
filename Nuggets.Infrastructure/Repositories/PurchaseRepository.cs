using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public class PurchaseOrderRepository(NuggetsDbContext db)
    : GenericRepository<PurchaseOrder>(db), IPurchaseOrderRepository
{
    public override async Task<(IReadOnlyList<PurchaseOrder> Items, int TotalCount)> GetPagedAsync(int page, int pageSize,
        IQueryable<PurchaseOrder>? startingQuery = null, CancellationToken ct = default)
    {
        startingQuery = db.PurchaseOrders
            .Include(ci => ci.Vendor)
            .Include(ci => ci.Lines)
            .Include(ci => ci.GoodsReceiptNotes)
            .ThenInclude(grn => grn.Lines)
            .Include(ci => ci.VendorBills)
            .ThenInclude(vb => vb.Lines)
            .AsNoTracking()
            .AsQueryable();
        return await base.GetPagedAsync(page, pageSize, startingQuery, ct);
    }
    
    public override async Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.PurchaseOrders
            .Include(so => so.Vendor)
            .Include(so => so.Lines)
            .Include(ci => ci.GoodsReceiptNotes)
            .ThenInclude(grn => grn.Lines)
            .Include(ci => ci.VendorBills)
            .ThenInclude(vb => vb.Lines)
            .FirstOrDefaultAsync(so => so.Id == id, ct);
    }

    public async Task<PurchaseOrder?> GetWithLinesAndGrnsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.PurchaseOrders
            .Include(po => po.Lines)
            .Include(po => po.GoodsReceiptNotes).ThenInclude(grn => grn.Lines)
            .FirstOrDefaultAsync(po => po.Id == id, ct);
    }

    public async Task<PurchaseOrder?> GetWithLinesAndBillsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.PurchaseOrders
            .Include(po => po.Lines)
            .Include(po => po.GoodsReceiptNotes).ThenInclude(grn => grn.Lines)
            .Include(po => po.VendorBills).ThenInclude(vb => vb.Lines)
            .FirstOrDefaultAsync(po => po.Id == id, ct);
    }
}

public class PurchaseReceiptRepository(NuggetsDbContext db)
    : GenericRepository<PurchaseReceipt>(db), IPurchaseReceiptRepository
{
    public override async Task<(IReadOnlyList<PurchaseReceipt> Items, int TotalCount)> GetPagedAsync(int page, int pageSize,
        IQueryable<PurchaseReceipt>? startingQuery = null, CancellationToken ct = default)
    {
        startingQuery = db.PurchaseReceipts
            .Include(ci => ci.Vendor)
            .Include(ci => ci.Lines)
            .AsNoTracking()
            .AsQueryable();
        return await base.GetPagedAsync(page, pageSize, startingQuery, ct);
    }
    
    public override async Task<PurchaseReceipt?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.PurchaseReceipts
            .Include(so => so.Vendor)
            .Include(so => so.Lines)
            .FirstOrDefaultAsync(so => so.Id == id, ct);
    }
}

public class VendorBillRepository(NuggetsDbContext db) : GenericRepository<VendorBill>(db), IVendorBillRepository
{
    public override async Task<(IReadOnlyList<VendorBill> Items, int TotalCount)> GetPagedAsync(int page, int pageSize,
        IQueryable<VendorBill>? startingQuery = null, CancellationToken ct = default)
    {
        startingQuery = db.VendorBills
            .Include(ci => ci.PurchaseOrder)
            .Include(ci => ci.Vendor)
            .Include(ci => ci.Lines)
            .AsNoTracking()
            .AsQueryable();
        return await base.GetPagedAsync(page, pageSize, startingQuery, ct);
    }
    
    public override async Task<VendorBill?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.VendorBills
            .Include(so => so.PurchaseOrder)
            .Include(so => so.VendorPayments)
            .Include(ci => ci.Vendor)
            .Include(so => so.Lines)
            .FirstOrDefaultAsync(so => so.Id == id, ct);
    }
    
    public async Task<VendorBill?> GetByIdWithLinesAsync(Guid id, CancellationToken ct = default)
    {
        return await db.VendorBills
            .Include(so => so.Lines)
            .FirstOrDefaultAsync(so => so.Id == id, ct);
    }
}

public class VendorPaymentRepository(NuggetsDbContext db)
    : GenericRepository<VendorPayment>(db), IVendorPaymentRepository
{
    public override async Task<(IReadOnlyList<VendorPayment> Items, int TotalCount)> GetPagedAsync(int page, int pageSize,
        IQueryable<VendorPayment>? startingQuery = null, CancellationToken ct = default)
    {
        startingQuery = db.VendorPayments
            .Include(ci => ci.VendorBill)
            .ThenInclude(vb => vb.Vendor)
            .AsNoTracking()
            .AsQueryable();
        return await base.GetPagedAsync(page, pageSize, startingQuery, ct);
    }
    
    public override async Task<VendorPayment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.VendorPayments
            .Include(ci => ci.VendorBill)
            .ThenInclude(vb => vb.Vendor)
            .FirstOrDefaultAsync(so => so.Id == id, ct);
    }
}