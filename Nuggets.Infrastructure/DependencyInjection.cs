using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.Services;
using Nuggets.Infrastructure.Identity;
using Nuggets.Infrastructure.Persistence;
using Nuggets.Infrastructure.Repositories;


namespace Nuggets.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var host = config["DB_HOST"] ?? "localhost";
        var port = config["DB_PORT"] ?? "5432";
        var db = config["DB_NAME"] ?? "nuggets_db";
        var user = config["DB_USER"] ?? "postgres";
        var password = config["DB_PASSWORD"] ?? "postgres";

        var cs = $"Host={host};Port={port};Database={db};Username={user};Password={password}";
        Console.WriteLine($"Using connection string: {cs}");

        services.AddDbContext<NuggetsDbContext>(o => o.UseNpgsql(cs, b => b.UseNodaTime()));

        services
            .AddIdentityCore<AppUser>(opt =>
            {
                opt.User.RequireUniqueEmail = true;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireDigit = false;
                opt.Password.RequiredLength = 5;
            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<NuggetsDbContext>()
            .AddDefaultTokenProviders();

        // Generic repo
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IUomRepository, UomRepository>();
        services.AddScoped<IUomConversionsRepository, UomConversionsRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        // services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IFoodMaterialRepository, FoodMaterialRepository>();
        services.AddScoped<IFoodRecipeRepository, FoodRecipeRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<IChartOfAccountRepository, ChartOfAccountRepository>();
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
        services.AddScoped<IJournalItemRepository, JournalItemRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IPurchaseReceiptRepository, PurchaseReceiptRepository>();
        services.AddScoped<IVendorBillRepository, VendorBillRepository>();
        services.AddScoped<IVendorPaymentRepository, VendorPaymentRepository>();
        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
        services.AddScoped<ISalesReceiptRepository, SalesReceiptRepository>();
        services.AddScoped<ICustomerInvoiceRepository, CustomerInvoiceRepository>();
        services.AddScoped<ICustomerPaymentRepository, CustomerPaymentRepository>();
        services.AddScoped<IGoodsReceiptNoteRepository, GoodsReceiptNoteRepository>();
        services.AddScoped<IDeliveryNoteRepository, DeliveryNoteRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        // Services
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IUomService, UomService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IVendorService, VendorService>();
        // services.AddScoped<IExpenseService, ExpenseService>();
        // services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IFoodMaterialService, FoodMaterialService>();
        services.AddScoped<IFoodRecipeService, FoodRecipeService>();
        services.AddScoped<IChartOfAccountService, ChartOfAccountService>();
        services.AddScoped<IJournalEntryService, JournalEntryService>();
        services.AddScoped<IJournalItemService, JournalItemService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IPurchaseReceiptService, PurchaseReceiptService>();
        services.AddScoped<IVendorBillService, VendorBillService>();
        services.AddScoped<IVendorPaymentService, VendorPaymentService>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<ISalesReceiptService, SalesReceiptService>();
        services.AddScoped<ICustomerInvoiceService, CustomerInvoiceService>();
        services.AddScoped<ICustomerPaymentService, CustomerPaymentService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IGoodsReceiptNoteService, GoodsReceiptNoteService>();
        services.AddScoped<IDeliveryNoteService, DeliveryNoteService>();
        services.AddScoped<ICompanyService, CompanyService>();
        
        // HttpContext
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }
}
