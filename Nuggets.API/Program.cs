using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.IdentityModel.Tokens;
using Nuggets.Domain.Authorization;
using Nuggets.Infrastructure;
using Nuggets.Infrastructure.Identity;
using Nuggets.Infrastructure.Options;
using Nuggets.Infrastructure.Persistence;
using Nuggets.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Nuggets API", Version = "v1" });
});


var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
                  ?? throw new Exception("Missing Jwt section in configuration");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.Key))
    };


    // Add this to support reading JWT from cookies
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // look for "jwt" cookie
            if (context.Request.Cookies.ContainsKey("jwt"))
            {
                context.Token = context.Request.Cookies["jwt"];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    foreach (var perm in RolePermissions.AllPermissions())
    {
        options.AddPolicy(perm, policy =>
        {
            // Policy passes if: user has Role w/ full access OR direct claim
            policy.RequireAssertion(ctx =>
                ctx.User.Claims.Any(c => 
                    c.Type == ClaimTypes.Role 
                    && RolePermissions.PermissionsByRole.ContainsKey(c.Value) 
                    && RolePermissions.PermissionsByRole[c.Value].Contains(perm)
                )
                ||
                ctx.User.HasClaim("permission", perm)
            );
        });
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // your React dev server
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // needed because you set cookies
        });
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // v1.0
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true; // returns headers like api-supported-versions
    options.ApiVersionReader = new UrlSegmentApiVersionReader(); // use /v1/
});

var app = builder.Build();


// --- Seed Admin User + Roles (Administration) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    var dbContext = services.GetRequiredService<NuggetsDbContext>();

    await IdentitySeeder.SeedDatabaseAsync(userManager, roleManager, dbContext);
}
// -------------------------------------------------


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nuggets API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
