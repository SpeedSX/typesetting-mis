using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class Equipment : TenantEntity
{
    [Required]
    public Guid CategoryId { get; set; }
    
    public EquipmentCategory? Category { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Model { get; set; }
    
    [MaxLength(255)]
    public string? SerialNumber { get; set; }
    
    [MaxLength(50)]
    public string Status { get; set; } = "active";
    
    public DateTime? PurchaseDate { get; set; }
    
    public decimal? PurchaseCost { get; set; }
    
    [MaxLength(255)]
    public string? Location { get; set; }
    
    public string? Notes { get; set; }
    
    // Navigation properties
    public ICollection<EquipmentCapability> Capabilities { get; set; } = new List<EquipmentCapability>();
}
