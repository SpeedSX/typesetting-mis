using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.Entities;

namespace TypesettingMIS.Infrastructure.Data;

public class DataSeeder(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
{
    public async Task SeedAsync()
    {
        // Check if system admin user already exists
        if (await context.Users.AnyAsync(u => u.Email == "admin@system.com"))
        {
            return; // System admin user already exists
        }

        // Create system-wide Admin role (not tied to any company)
        var systemAdminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            NormalizedName = "ADMIN",
            CompanyId = null, // System-wide role
            Description = "System Administrator - Access to all companies",
            Permissions = "[\"manage_companies\",\"manage_users\",\"manage_customers\",\"manage_orders\",\"manage_quotes\",\"manage_invoices\",\"view_reports\",\"manage_settings\"]",
            IsSystem = true,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        context.Roles.Add(systemAdminRole);

        // Create system admin user (not tied to any company)
        var systemAdminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@system.com",
            NormalizedEmail = "ADMIN@SYSTEM.COM",
            UserName = "admin@system.com",
            NormalizedUserName = "ADMIN@SYSTEM.COM",
            FirstName = "System",
            LastName = "Admin",
            CompanyId = null, // Not tied to any company
            RoleId = systemAdminRole.Id,
            IsActive = true,
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        systemAdminUser.PasswordHash = passwordHasher.HashPassword(systemAdminUser, "Admin123!");
        context.Users.Add(systemAdminUser);

        // Create test company with fixed GUID for testing
        var company = new Company
        {
            Id = new Guid("11111111-1111-1111-1111-111111111111"),
            Name = "Test Company",
            Domain = "testcompany.com",
            Settings = "{\"timezone\":\"UTC\",\"currency\":\"USD\"}",
            SubscriptionPlan = "basic",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync();

        // Create company-scoped User role
        var userRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "User",
            NormalizedName = "USER",
            CompanyId = company.Id, // Company-scoped role
            Description = "Regular User - Company scoped",
            Permissions = "[\"manage_customers\",\"manage_orders\",\"manage_quotes\",\"view_reports\"]",
            IsSystem = false,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        context.Roles.Add(userRole);

        // Create company-scoped Admin role (for company administrators)
        var companyAdminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "CompanyAdmin",
            NormalizedName = "COMPANYADMIN",
            CompanyId = company.Id, // Company-scoped role
            Description = "Company Administrator - Manages company users and settings",
            Permissions = "[\"manage_users\",\"manage_customers\",\"manage_orders\",\"manage_quotes\",\"manage_invoices\",\"view_reports\",\"manage_settings\"]",
            IsSystem = false,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        context.Roles.Add(companyAdminRole);
        await context.SaveChangesAsync();

        // Create test company admin user
        var companyAdminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@testcompany.com",
            NormalizedEmail = "ADMIN@TESTCOMPANY.COM",
            UserName = "admin@testcompany.com",
            NormalizedUserName = "ADMIN@TESTCOMPANY.COM",
            FirstName = "Company",
            LastName = "Admin",
            CompanyId = company.Id, // Tied to test company
            RoleId = companyAdminRole.Id,
            IsActive = true,
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        companyAdminUser.PasswordHash = passwordHasher.HashPassword(companyAdminUser, "Admin123!");
        context.Users.Add(companyAdminUser);

        // Create test regular user
        var regularUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@testcompany.com",
            NormalizedEmail = "USER@TESTCOMPANY.COM",
            UserName = "user@testcompany.com",
            NormalizedUserName = "USER@TESTCOMPANY.COM",
            FirstName = "Test",
            LastName = "User",
            CompanyId = company.Id, // Tied to test company
            RoleId = userRole.Id,
            IsActive = true,
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        regularUser.PasswordHash = passwordHasher.HashPassword(regularUser, "User123!");
        context.Users.Add(regularUser);

        await context.SaveChangesAsync();
    }
}
