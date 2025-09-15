using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TypesettingMIS.Infrastructure;
using TypesettingMIS.Infrastructure.Middleware;
using TypesettingMIS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using TypesettingMIS.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
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
