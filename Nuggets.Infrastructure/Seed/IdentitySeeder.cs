using Microsoft.AspNetCore.Identity;
using Nuggets.Domain.Entities;
using Nuggets.Infrastructure.Identity;
using Nuggets.Infrastructure.Persistence;

namespace Nuggets.Infrastructure.Seed;

public static class IdentitySeeder
{
    private const string DefaultAdminEmail = "admin@example.com";
    private const string DefaultAdminUserName = "admin";
    private const string DefaultAdminPassword = "admin"; // change in prod!

    // Roles
    private static readonly string[] Roles = new[]
    {
        "Admin",
        "Sales",
        "Warehouse"
    };

    public static async Task SeedDatabaseAsync(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        NuggetsDbContext dbContext)
    {
        // Ensure roles exist
        foreach (var roleName in Roles)
        {
            if (await roleManager.RoleExistsAsync(roleName)) continue;
            var role = new AppRole { Name = roleName };
            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        // Ensure default admin user exists
        var adminUser = await userManager.FindByNameAsync(DefaultAdminUserName);
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = DefaultAdminUserName,
                Email = DefaultAdminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, DefaultAdminPassword);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create default admin user: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Ensure admin user has Admin role
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        
        

        // Ensure default Company exists
        var defaultCompany = dbContext.Companies.FirstOrDefault(c => c.Name == "PT. Trading Nuggets Indonesia");
        if (defaultCompany == null)
        {
            defaultCompany = new Company
            {
                Name = "PT. Trading Nuggets Indonesia",
                LegalName = "PT. Trading Nuggets Indonesia",
                NPWP = "-",
                PKP = true, // this company is VAT registered
                City = "Jakarta",
                Province = "DKI Jakarta",
                Country = "Indonesia",
                Email = "info@nuggetsvape.com",
                Phone = "-"
            };

            dbContext.Companies.Add(defaultCompany);
            await dbContext.SaveChangesAsync();
        }

        // 4️⃣ Ensure UserCompany link exists
        var alreadyLinked = dbContext.UserCompanies
            .Any(uc => uc.UserId == adminUser.Id && uc.CompanyId == defaultCompany.Id);

        if (!alreadyLinked)
        {
            var link = new UserCompany
            {
                UserId = adminUser.Id,
                CompanyId = defaultCompany.Id,
            };
            dbContext.UserCompanies.Add(link);
            await dbContext.SaveChangesAsync();
        }
    }
}