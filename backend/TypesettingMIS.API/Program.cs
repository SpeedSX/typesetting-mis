using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json;
using TypesettingMIS.Infrastructure;
using TypesettingMIS.Infrastructure.Middleware;
using TypesettingMIS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using TypesettingMIS.Core.Entities;
using TypesettingMIS.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
var devOrigin = builder.Configuration["Cors:DevOrigin"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (allowedOrigins is { Length: > 0 })
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins(devOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            throw new InvalidOperationException("Cors:AllowedOrigins is not configured for Production");
        }
    });
});

// Add HttpContextAccessor for tenant context
builder.Services.AddHttpContextAccessor();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Use centralized JWT configuration
        options.TokenValidationParameters = JwtConfigurationService.GetTokenValidationParameters(builder.Configuration);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Add tenant resolution middleware
app.UseMiddleware<TenantResolutionMiddleware>();

app.MapControllers();

// Seed initial data on startup (dev or explicit opt-in)
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Seed:Enabled"))
{
    using var scope = app.Services.CreateScope();
    var logger = app.Logger;
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    var seeder = new DataSeeder(context, passwordHasher);
    try
    {
        await seeder.SeedAsync();
        logger.LogInformation("Initial data seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error seeding data");
    }
}

app.Run();
