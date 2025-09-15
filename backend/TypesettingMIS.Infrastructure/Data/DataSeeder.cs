using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.Entities;

namespace TypesettingMIS.Infrastructure.Data;

public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public DataSeeder(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        // Check if admin user already exists
        if (await _context.Users.AnyAsync(u => u.Email == "admin@testcompany.com"))
        {
            return; // Admin user already exists
        }

        // Create test company
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = "Test Company",
            Domain = "testcompany.com",
            Settings = "{\"timezone\":\"UTC\",\"currency\":\"USD\"}",
            SubscriptionPlan = "basic",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        // Create Admin role
        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            NormalizedName = "ADMIN",
            CompanyId = company.Id,
            Description = "System Administrator",
            Permissions = "[\"manage_companies\",\"manage_users\",\"manage_customers\",\"manage_orders\",\"manage_quotes\",\"manage_invoices\",\"view_reports\",\"manage_settings\"]",
            IsSystem = true,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        _context.Roles.Add(adminRole);

        // Create User role
        var userRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "User",
            NormalizedName = "USER",
            CompanyId = company.Id,
            Description = "Regular User",
            Permissions = "[\"manage_customers\",\"manage_orders\",\"manage_quotes\",\"view_reports\"]",
            IsSystem = false,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        _context.Roles.Add(userRole);
        await _context.SaveChangesAsync();

        // Create admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@testcompany.com",
            NormalizedEmail = "ADMIN@TESTCOMPANY.COM",
            UserName = "admin@testcompany.com",
            NormalizedUserName = "ADMIN@TESTCOMPANY.COM",
            FirstName = "Admin",
            LastName = "User",
            CompanyId = company.Id,
            RoleId = adminRole.Id,
            IsActive = true,
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, "Admin123!");
        _context.Users.Add(adminUser);

        await _context.SaveChangesAsync();
    }
}
