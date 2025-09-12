using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class Quote : TenantEntity
{
    [Required]
    [MaxLength(50)]
    public string QuoteNumber { get; set; } = string.Empty;
    
    [Required]
    public Guid CustomerId { get; set; }
    
    public Customer? Customer { get; set; }
    
    [MaxLength(50)]
    public string Status { get; set; } = "draft"; // draft, sent, accepted, rejected, expired
    
    [Required]
    public decimal TotalAmount { get; set; } = 0;
    
    public DateTime? ValidUntil { get; set; }
    
    public string? Notes { get; set; }
    
    [Required]
    public Guid CreatedById { get; set; }
    
    public User? CreatedBy { get; set; }
    
    // Navigation properties
    public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
