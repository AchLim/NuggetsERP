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
        var cs = config.GetConnectionString("DefaultConnection")
                 ?? "Host=localhost;Port=5432;Database=nuggets_db;Username=postgres;Password=postgres";

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
        services.AddScoped<IProductUomRepository, ProductUomRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IFoodMaterialRepository, FoodMaterialRepository>();
        services.AddScoped<IFoodRecipeRepository, FoodRecipeRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        // Services
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IFoodMaterialService, FoodMaterialService>();
        services.AddScoped<IFoodRecipeService, FoodRecipeService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<ICompanyService, CompanyService>();
        
        // HttpContext
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }
}
