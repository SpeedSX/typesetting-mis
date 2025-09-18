using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.Entities;

namespace TypesettingMIS.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User, Role, Guid>(options)
{
    // Shared entities (not tenant-specific)
    public DbSet<Company> Companies { get; set; }
    public DbSet<EquipmentCategory> EquipmentCategories { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Invitation> Invitations { get; set; }

    // Tenant-specific entities
    public DbSet<Equipment> Equipment { get; set; }
    public DbSet<EquipmentCapability> EquipmentCapabilities { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<QuoteItem> QuoteItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Inventory> Inventory { get; set; }
    public DbSet<PricingRule> PricingRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure custom table names for ASP.NET Core Identity
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().ToTable("UserTokens");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        // Configure Company
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Create case-insensitive unique index using PostgreSQL expression
            entity.HasIndex(e => e.Domain)
                .IsUnique()
                .HasDatabaseName("IX_Companies_Domain_Unique")
                .HasMethod("btree")
                .HasAnnotation("Npgsql:IndexExpression", "UPPER(\"Domain\")");
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Domain).IsRequired().HasMaxLength(255);
            entity.Property(e => e.SubscriptionPlan).HasMaxLength(50).HasDefaultValue("basic");
        });

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.NormalizedEmail }).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => new { e.CompanyId, e.NormalizedName }).IsUnique();
            
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Equipment
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Model).HasMaxLength(255);
            entity.Property(e => e.SerialNumber).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("active");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.PurchaseCost).HasPrecision(12, 2);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.Equipment)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Equipment)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure EquipmentCategory
        modelBuilder.Entity<EquipmentCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        // Configure EquipmentCapability
        modelBuilder.Entity<EquipmentCapability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CapabilityName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Unit).HasMaxLength(50);
            
            entity.HasOne(e => e.Equipment)
                .WithMany(eq => eq.Capabilities)
                .HasForeignKey(e => e.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Service
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BasePrice).HasPrecision(12, 2);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.Services)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BasePrice).HasPrecision(12, 2);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.TaxId).HasMaxLength(100);
            
            entity.HasOne(e => e.Company)
                .WithMany(c => c.Customers)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Quote
        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyId, e.QuoteNumber }).IsUnique();
            entity.Property(e => e.QuoteNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("draft");
            entity.Property(e => e.TotalAmount).HasPrecision(12, 2).HasDefaultValue(0);
            
            entity.HasOne(e => e.Company)
                .WithMany(c => c.Quotes)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Quotes)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.CreatedBy)
                .WithMany(u => u.CreatedQuotes)
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure QuoteItem
        modelBuilder.Entity<QuoteItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ItemType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Quantity).HasPrecision(10, 3);
            entity.Property(e => e.UnitPrice).HasPrecision(12, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(12, 2);
            
            entity.HasOne(e => e.Quote)
                .WithMany(q => q.Items)
                .HasForeignKey(e => e.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyId, e.OrderNumber }).IsUnique();
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("pending");
            entity.Property(e => e.TotalAmount).HasPrecision(12, 2).HasDefaultValue(0);
            
            entity.HasOne(e => e.Company)
                .WithMany(c => c.Orders)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Quote)
                .WithMany(q => q.Orders)
                .HasForeignKey(e => e.QuoteId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.CreatedBy)
                .WithMany(u => u.CreatedOrders)
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ItemType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Quantity).HasPrecision(10, 3);
            entity.Property(e => e.UnitPrice).HasPrecision(12, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(12, 2);
            
            entity.HasOne(e => e.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Invoice
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyId, e.InvoiceNumber }).IsUnique();
            entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("draft");
            entity.Property(e => e.Amount).HasPrecision(12, 2);
            
            entity.HasOne(e => e.Company)
                .WithMany(c => c.Invoices)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Invoices)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Order)
                .WithMany(o => o.Invoices)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.CreatedBy)
                .WithMany(u => u.CreatedInvoices)
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Inventory
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ItemName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ItemType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Quantity).HasPrecision(10, 3).HasDefaultValue(0);
            entity.Property(e => e.UnitCost).HasPrecision(12, 2);
            entity.Property(e => e.ReorderPoint).HasPrecision(10, 3).HasDefaultValue(0);
            entity.Property(e => e.Supplier).HasMaxLength(255);
            entity.Property(e => e.Location).HasMaxLength(255);
            
            entity.HasOne(e => e.Company)
                .WithMany(c => c.Inventory)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure PricingRule
        modelBuilder.Entity<PricingRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.RuleType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Conditions).IsRequired();
            entity.Property(e => e.Calculation).IsRequired();
            
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.RevokedByIp).HasMaxLength(50);
            entity.Property(e => e.ReplacedByTokenHash).HasMaxLength(500);
            entity.Property(e => e.ReasonRevoked).HasMaxLength(100);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Invitation
        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.Token).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UsedByEmail).HasMaxLength(256);
            
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure base entity properties for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.CreatedAt))
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.UpdatedAt))
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            }
        }
    }
}
