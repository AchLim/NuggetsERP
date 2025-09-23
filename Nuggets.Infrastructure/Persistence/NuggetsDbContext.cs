using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Identity;

namespace Nuggets.Infrastructure.Persistence;

public class NuggetsDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public NuggetsDbContext(DbContextOptions<NuggetsDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<UnitOfMeasure> Uoms => Set<UnitOfMeasure>();
    public DbSet<UnitOfMeasureConversion> UomConversions => Set<UnitOfMeasureConversion>();

    public DbSet<GoodsReceiptNote> GoodsReceiptNotes => Set<GoodsReceiptNote>();
    public DbSet<DeliveryNote> DeliveryNotes => Set<DeliveryNote>();
    
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<SalesReceipt> SalesReceipts => Set<SalesReceipt>();
    public DbSet<SalesReceiptLine> SalesReceiptLines => Set<SalesReceiptLine>();
    public DbSet<CustomerInvoice> CustomerInvoices => Set<CustomerInvoice>();
    public DbSet<CustomerInvoiceLine> CustomerInvoiceLines => Set<CustomerInvoiceLine>();
    public DbSet<CustomerPayment> CustomerPayments => Set<CustomerPayment>();

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<PurchaseReceipt> PurchaseReceipts => Set<PurchaseReceipt>();
    public DbSet<PurchaseReceiptLine> PurchaseReceiptLines => Set<PurchaseReceiptLine>();
    public DbSet<VendorPayment> VendorPayments => Set<VendorPayment>();
    public DbSet<VendorBill> VendorBills => Set<VendorBill>();
    public DbSet<VendorBillLine> VendorBillLines => Set<VendorBillLine>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<ChartOfAccount> ChartOfAccounts => Set<ChartOfAccount>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalItem> JournalItems => Set<JournalItem>();

    // Food
    public DbSet<FoodMaterial> FoodMaterials => Set<FoodMaterial>();
    public DbSet<FoodRecipe> FoodRecipes => Set<FoodRecipe>();

    // Multi-company
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Apply global filters automatically to all BaseEntity descendants
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)) continue;
            
            var parameter = Expression.Parameter(entityType.ClrType, "e");

            // e => !e.IsDeleted && e.Active
            var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var activeProperty = Expression.Property(parameter, nameof(BaseEntity.Active));

            var isNotDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            var isActive = Expression.Equal(activeProperty, Expression.Constant(true));

            var finalExpr = Expression.Lambda(
                Expression.AndAlso(isNotDeleted, isActive),
                parameter
            );

            builder.Entity(entityType.ClrType).HasQueryFilter(finalExpr);
        }
        
        builder.Entity<Customer>(b =>
        {
            b.HasKey(x => x.Id);
        });
        
        builder.Entity<Vendor>(b =>
        {
            b.HasKey(x => x.Id);
        });
        
        builder.Entity<Product>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(512);

            b.HasOne(x => x.Vendor)
             .WithMany()
             .HasForeignKey(x => x.VendorId)
             .OnDelete(DeleteBehavior.Restrict);
            
            b.HasOne(x => x.Uom)
                .WithMany()
                .HasForeignKey(x => x.UomId);
            b.HasOne(x => x.ProductCategory)
                .WithMany()
                .HasForeignKey(x => x.ProductCategoryId);
            
            b.HasMany(p => p.StockMovements)
                .WithOne(sm => sm.Product)
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductCategory>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Parent)
                .WithMany()
                .HasForeignKey(x => x.ParentId);
        });

        builder.Entity<UnitOfMeasure>(b =>
        {
            b.HasKey(x => x.Id);
        });

        builder.Entity<UnitOfMeasureConversion>().ToTable("uom_conversion");

        builder.Entity<UnitOfMeasureConversion>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.FromUom)
                .WithMany(x => x.FromConversions)
                .HasForeignKey(x => x.FromUomId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ToUom)
                .WithMany(x => x.ToConversions)
                .HasForeignKey(x => x.ToUomId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.FromUomId, x.ToUomId }).IsUnique();
        });
        
        builder.Entity<SalesOrder>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            b.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId);
        });

        builder.Entity<SalesOrderLine>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(l => l.SalesOrder)
                .WithMany(o => o.Lines)
                .HasForeignKey(l => l.SalesOrderId);
            b.HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId);
            b.HasOne(l => l.Uom)
                .WithMany()
                .HasForeignKey(l => l.UomId);
        });
        
        builder.Entity<SalesReceipt>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(32);
            b.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId);
        });

        builder.Entity<SalesReceiptLine>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(l => l.SalesReceipt)
                .WithMany(o => o.Lines)
                .HasForeignKey(l => l.SalesReceiptId);
            b.HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId);
            b.HasOne(l => l.Uom)
                .WithMany()
                .HasForeignKey(l => l.UomId);
        });
        
        builder.Entity<CustomerInvoice>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(32);
            b.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId);
        });

        builder.Entity<CustomerInvoiceLine>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(l => l.CustomerInvoice)
                .WithMany(o => o.Lines)
                .HasForeignKey(l => l.CustomerInvoiceId);
            b.HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId);
            b.HasOne(l => l.Uom)
                .WithMany()
                .HasForeignKey(l => l.UomId);
        });
        
        builder.Entity<CustomerPayment>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.CustomerInvoice)
                .WithMany(ci => ci.CustomerPayments)
                .HasForeignKey(x => x.CustomerInvoiceId);
        });
        
        builder.Entity<PurchaseOrder>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(32);
            b.HasOne(x => x.Vendor)
                .WithMany()
                .HasForeignKey(x => x.VendorId);
        });

        builder.Entity<PurchaseOrderLine>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(l => l.PurchaseOrder)
                .WithMany(o => o.Lines)
                .HasForeignKey(l => l.PurchaseOrderId);
            b.HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId);
            b.HasOne(l => l.Uom)
                .WithMany()
                .HasForeignKey(l => l.UomId);
        });
        
        builder.Entity<PurchaseReceipt>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(32);
            b.HasOne(x => x.Vendor)
                .WithMany()
                .HasForeignKey(x => x.VendorId);
        });

        builder.Entity<PurchaseReceiptLine>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(l => l.PurchaseReceipt)
                .WithMany(o => o.Lines)
                .HasForeignKey(l => l.PurchaseReceiptId);
            b.HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId);
            b.HasOne(l => l.Uom)
                .WithMany()
                .HasForeignKey(l => l.UomId);
        });
        
        builder.Entity<VendorPayment>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.VendorBill)
                .WithMany(ci => ci.VendorPayments)
                .HasForeignKey(x => x.VendorBillId);
        });
        
        builder.Entity<VendorBill>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(32);
            b.HasOne(x => x.Vendor)
                .WithMany()
                .HasForeignKey(x => x.VendorId);
        });

        builder.Entity<VendorBillLine>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(l => l.VendorBill)
                .WithMany(o => o.Lines)
                .HasForeignKey(l => l.VendorBillId);
            b.HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId);
            b.HasOne(l => l.Uom)
                .WithMany()
                .HasForeignKey(l => l.UomId);
        });

        // --- Food Recipe ---
        builder.Entity<FoodRecipe>(b =>
        {
            b.HasKey(x => x.Id);

            b.HasOne(x => x.Product)
             .WithMany(p => p.FoodRecipes)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.FoodMaterial)
             .WithMany(m => m.FoodRecipes)
             .HasForeignKey(x => x.FoodMaterialId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Uom)
             .WithMany()
             .HasForeignKey(x => x.UomId)
             .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.Quantity).HasColumnType("numeric(18,3)");
        });

        // Chart of Accounts
        builder.Entity<ChartOfAccount>(b =>
        {
            b.HasKey(x => x.Id);
        });
        builder.Entity<JournalEntry>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasMany(j => j.Items)
                .WithOne(i => i.JournalEntry)
                .HasForeignKey(i => i.JournalEntryId);
        });
        builder.Entity<JournalItem>(b =>
        {
            b.HasOne(i => i.Account)
                .WithMany()
                .HasForeignKey(i => i.AccountId);
        });

        // --- Inventory Movements ---
        builder.Entity<StockMovement>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasKey(x => x.Id);
            b.Property(x => x.MovementType)
                .HasConversion<string>()
                .HasMaxLength(32);
            b.HasOne(x => x.Product)
                .WithMany(p => p.StockMovements)
                .HasForeignKey(x => x.ProductId);
        });

        // --- Companies ---
        builder.Entity<Company>().ToTable("res_company");
        builder.Entity<UserCompany>().HasKey(uc => new { uc.UserId, uc.CompanyId });

        builder.Entity<UserCompany>()
            .HasOne<AppUser>()
            .WithMany(u => u.UserCompanies)
            .HasForeignKey(uc => uc.UserId);

        builder.Entity<UserCompany>()
            .HasOne(uc => uc.Company)
            .WithMany(c => c.UserCompanies)
            .HasForeignKey(uc => uc.CompanyId);
        
        builder.Entity<UserCompany>()
            .HasQueryFilter(uc => uc.Company.Active && !uc.Company.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var e in entries)
        {
            if (e.State == EntityState.Modified)
                e.Entity.UpdatedAtUtc = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(ct);
    }
}
