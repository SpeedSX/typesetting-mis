using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class Invoice : TenantEntity
{
    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;
    
    [Required]
    public Guid CustomerId { get; set; }
    
    public Customer? Customer { get; set; }
    
    public Guid? OrderId { get; set; }
    
    public Order? Order { get; set; }
    
    [Required]
    public decimal Amount { get; set; }
    
    [MaxLength(50)]
    public string Status { get; set; } = "draft"; // draft, sent, paid, overdue
    
    [Required]
    public DateTime DueDate { get; set; }
    
    public DateTime? PaidDate { get; set; }
    
    [Required]
    public Guid CreatedById { get; set; }
    
    public User? CreatedBy { get; set; }
}
