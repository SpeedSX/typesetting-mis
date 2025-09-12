using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class Product : TenantEntity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public decimal BasePrice { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<QuoteItem> QuoteItems { get; set; } = new List<QuoteItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
