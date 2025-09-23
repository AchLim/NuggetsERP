using System.Net;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nuggets.Domain.Authorization;
using Nuggets.Infrastructure;
using Nuggets.Infrastructure.Identity;
using Nuggets.Infrastructure.Options;
using Nuggets.Infrastructure.Persistence;
using Nuggets.Infrastructure.Seed;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load();  // Load .env only in dev
}

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Nuggets API", Version = "v1" });
});

var jwtOptions = new JwtOptions
{
    Issuer = builder.Configuration["JWT_ISSUER"] ?? "nuggets-api",
    Audience = builder.Configuration["JWT_AUDIENCE"] ?? "nuggets-users",
    Key = builder.Configuration["JWT_KEY"] ?? "super_secret_dev_key"
};

builder.Services.Configure<JwtOptions>(opts => {
    opts.Issuer = jwtOptions.Issuer;
    opts.Audience = jwtOptions.Audience;
    opts.Key = jwtOptions.Key;
});

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
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtOptions.Key))
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

builder.Services.AddHealthChecks();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    
    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.16.0.0"), 12));
});

var app = builder.Build();

app.MapHealthChecks("/health");

// --- Seed Admin User + Roles (Administration) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<NuggetsDbContext>();
    
    await dbContext.Database.MigrateAsync();
    
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

    await IdentitySeeder.SeedDatabaseAsync(userManager, roleManager, dbContext);
    await UomSeeder.SeedUomsAsync(dbContext);
    await AccountSeeder.SeedChartOfAccountsAsync(dbContext);

    // --- SEQUENCES (PostgreSQL) ---
    await SequenceSeeder.EnsureSequencesAsync(dbContext);
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

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
