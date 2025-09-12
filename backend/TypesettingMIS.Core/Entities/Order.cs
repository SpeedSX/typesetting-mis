using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class Order : TenantEntity
{
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    
    [Required]
    public Guid CustomerId { get; set; }
    
    public Customer? Customer { get; set; }
    
    public Guid? QuoteId { get; set; }
    
    public Quote? Quote { get; set; }
    
    [MaxLength(50)]
    public string Status { get; set; } = "pending"; // pending, in_progress, completed, cancelled
    
    [Required]
    public decimal TotalAmount { get; set; } = 0;
    
    public DateTime? DueDate { get; set; }
    
    public string? Notes { get; set; }
    
    [Required]
    public Guid CreatedById { get; set; }
    
    public User? CreatedBy { get; set; }
    
    // Navigation properties
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
