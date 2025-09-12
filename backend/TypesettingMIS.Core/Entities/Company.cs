using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class Company : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Domain { get; set; } = string.Empty;
    
    public string? Settings { get; set; } // JSON string for company-specific settings
    
    [MaxLength(50)]
    public string SubscriptionPlan { get; set; } = "basic";
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
}
