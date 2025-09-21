using Microsoft.EntityFrameworkCore;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Repositories;

public class SalesOrderRepository(NuggetsDbContext db) : GenericRepository<SalesOrder>(db), ISalesOrderRepository
{
    public override async Task<(IReadOnlyList<SalesOrder> Items, int TotalCount)> GetPagedAsync(int page, int pageSize,
        IQueryable<SalesOrder>? startingQuery = null, CancellationToken ct = default)
    {
        startingQuery = db.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.Lines)
            .AsNoTracking()
            .AsQueryable();
        return await base.GetPagedAsync(page, pageSize, startingQuery, ct);
    }

    public override async Task<SalesOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.Lines)
            .FirstOrDefaultAsync(so => so.Id == id, ct);
    }
    
    

    public async Task<SalesOrder?> GetWithLinesAndDnsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.SalesOrders
            .Include(so => so.Lines)
            .Include(so => so.DeliveryNotes).ThenInclude(grn => grn.Lines)
            .FirstOrDefaultAsync(so => so.Id == id, ct);
    }

    public async Task<SalesOrder?> GetWithLinesAndInvoicesAsync(Guid id, CancellationToken ct = default)
    {
        return await db.SalesOrders
            .Include(so => so.Lines)
            .Include(so => so.DeliveryNotes).ThenInclude(grn => grn.Lines)
            .Include(so => so.CustomerInvoices).ThenInclude(vb => vb.Lines)
            .FirstOrDefaultAsync(so => so.Id == id, ct);
    }
}

public class CustomerInvoiceRepository(NuggetsDbContext db)
    : GenericRepository<CustomerInvoice>(db), ICustomerInvoiceRepository
{
    public override async Task<(IReadOnlyList<CustomerInvoice> Items, int TotalCount)> GetPagedAsync(int page, int pageSize,
        IQueryable<CustomerInvoice>? startingQuery = null, CancellationToken ct = default)
    {
        startingQuery = db.CustomerInvoices
            .Include(ci => ci.Customer)
            .Include(ci => ci.SalesOrder)
            .Include(ci => ci.Lines)
            .AsNoTracking()
            .AsQueryable();
        return await base.GetPagedAsync(page, pageSize, startingQuery, ct);
    }

    public override async Task<CustomerInvoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.CustomerInvoices
            .Include(ci => ci.Customer)
            .Include(ci => ci.SalesOrder)
            .Include(ci => ci.Lines)
            .FirstOrDefaultAsync(ci => ci.Id == id, ct);
    }

    public async Task<CustomerInvoice?> GetByIdWithLinesAsync(Guid id, CancellationToken ct = default)
    {
        return await db.CustomerInvoices
            .Include(ci => ci.Lines)
            .FirstOrDefaultAsync(ci => ci.Id == id, ct);
    }
}

public class SalesReceiptRepository(NuggetsDbContext db) : GenericRepository<SalesReceipt>(db), ISalesReceiptRepository
{
    public override async Task<(IReadOnlyList<SalesReceipt> Items, int TotalCount)> GetPagedAsync(int page, int pageSize,
        IQueryable<SalesReceipt>? startingQuery = null, CancellationToken ct = default)
    {
        startingQuery = db.SalesReceipts
            .Include(sr => sr.Customer)
            .Include(sr => sr.Lines)
            .AsNoTracking()
            .AsQueryable();
        return await base.GetPagedAsync(page, pageSize, startingQuery, ct);
    }

    public override async Task<SalesReceipt?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.SalesReceipts
            .Include(sr => sr.Customer)
            .Include(sr => sr.Lines)
            .FirstOrDefaultAsync(sr => sr.Id == id, ct);
    }
}

public class CustomerPaymentRepository(NuggetsDbContext db)
    : GenericRepository<CustomerPayment>(db), ICustomerPaymentRepository
{
    public override async Task<(IReadOnlyList<CustomerPayment> Items, int TotalCount)> GetPagedAsync(int page, int pageSize,
        IQueryable<CustomerPayment>? startingQuery = null, CancellationToken ct = default)
    {
        startingQuery = db.CustomerPayments
            .Include(cp => cp.CustomerInvoice)
            .ThenInclude(ci => ci.Customer)
            .AsNoTracking()
            .AsQueryable();
        return await base.GetPagedAsync(page, pageSize, startingQuery, ct);
    }

    public override async Task<CustomerPayment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.CustomerPayments
            .Include(cp => cp.CustomerInvoice)
            .ThenInclude(ci => ci.Customer)
            .FirstOrDefaultAsync(so => so.Id == id, ct);
    }
}