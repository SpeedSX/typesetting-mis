using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class Inventory : TenantEntity
{
    [Required]
    [MaxLength(255)]
    public string ItemName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string ItemType { get; set; } = string.Empty; // material, supply, tool
    
    [Required]
    public decimal Quantity { get; set; } = 0;
    
    [Required]
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;
    
    public decimal? UnitCost { get; set; }
    
    public decimal ReorderPoint { get; set; } = 0;
    
    [MaxLength(255)]
    public string? Supplier { get; set; }
    
    [MaxLength(255)]
    public string? Location { get; set; }
}
