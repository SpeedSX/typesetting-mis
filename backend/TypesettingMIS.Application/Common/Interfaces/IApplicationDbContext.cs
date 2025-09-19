using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.Entities;

namespace TypesettingMIS.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Company> Companies { get; }
    DbSet<Role> Roles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Invitation> Invitations { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Equipment> Equipment { get; }
    DbSet<EquipmentCapability> EquipmentCapabilities { get; }
    DbSet<Service> Services { get; }
    DbSet<Product> Products { get; }
    DbSet<Quote> Quotes { get; }
    DbSet<QuoteItem> QuoteItems { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Inventory> Inventory { get; }
    DbSet<PricingRule> PricingRules { get; }
    DbSet<EquipmentCategory> EquipmentCategories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}