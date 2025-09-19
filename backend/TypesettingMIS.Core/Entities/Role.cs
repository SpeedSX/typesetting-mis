using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class Role : IdentityRole<Guid>
{
    public Guid? CompanyId { get; set; } // Nullable for system roles (Admin)
    public Company? Company { get; set; }
    
    public string? Description { get; set; }
    
    public string? Permissions { get; set; } // JSON string for permissions array
    
    public bool IsSystem { get; set; } = false;
    
    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
}
