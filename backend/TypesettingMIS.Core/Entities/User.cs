using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class User : IdentityUser<Guid>
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    
    public Guid RoleId { get; set; }
    
    public Role? Role { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? LastLogin { get; set; }
    
    // Navigation properties
    public ICollection<Quote> CreatedQuotes { get; set; } = new List<Quote>();
    public ICollection<Order> CreatedOrders { get; set; } = new List<Order>();
    public ICollection<Invoice> CreatedInvoices { get; set; } = new List<Invoice>();
}
