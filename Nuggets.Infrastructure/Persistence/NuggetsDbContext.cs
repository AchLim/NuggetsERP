using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Identity;

namespace Nuggets.Infrastructure.Persistence;

public class NuggetsDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public NuggetsDbContext(DbContextOptions<NuggetsDbContext> options) : base(options) { }

    // Master Tables
    public DbSet<UnitOfMeasure> Uoms => Set<UnitOfMeasure>();
    public DbSet<UnitOfMeasureConversion> UomConversions => Set<UnitOfMeasureConversion>();

    // Materials & Recipes
    public DbSet<FoodMaterial> FoodMaterials => Set<FoodMaterial>();
    public DbSet<FoodRecipe> FoodRecipes => Set<FoodRecipe>();

    // Products & Sales
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductUom> ProductUoms => Set<ProductUom>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<Expense> Expenses => Set<Expense>();

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
        
        // --- Products ---
        builder.Entity<Product>().ToTable("product_product");
        builder.Entity<ProductCategory>().ToTable("product_category");
        builder.Entity<ProductUom>().ToTable("product_uom");
        builder.Entity<Customer>().ToTable("customer_customer");

        builder.Entity<Product>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Price).HasColumnType("numeric(18,4)");
            b.Property(x => x.Name).IsRequired().HasMaxLength(512);

            b.HasOne(x => x.Supplier)
             .WithMany()
             .HasForeignKey(x => x.SupplierId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductCategory>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);

            b.HasOne(x => x.Parent)
             .WithMany()
             .HasForeignKey(x => x.ParentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Uoms (Units of Measure) ---
        builder.Entity<UnitOfMeasure>().ToTable("uom_uom");

        builder.Entity<UnitOfMeasure>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Abbreviation).IsRequired().HasMaxLength(16);
        });

        builder.Entity<UnitOfMeasureConversion>().ToTable("uom_conversion");

        builder.Entity<UnitOfMeasureConversion>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.ConversionRate).HasColumnType("numeric(18,6)");

            b.HasOne(x => x.FromUom)
             .WithMany(u => u.FromConversions)
             .HasForeignKey(x => x.FromUomId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ToUom)
             .WithMany(u => u.ToConversions)
             .HasForeignKey(x => x.ToUomId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.FromUomId, x.ToUomId }).IsUnique();
        });

        // --- Food Recipe ---
        builder.Entity<FoodRecipe>().ToTable("mrp_bom");

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

        builder.Entity<Expense>().ToTable("account_expense");

        // Composite Key for Many-to-Many
        builder.Entity<Company>().ToTable("res_company");
        builder.Entity<UserCompany>()
            .HasKey(uc => new { uc.UserId, uc.CompanyId });

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
