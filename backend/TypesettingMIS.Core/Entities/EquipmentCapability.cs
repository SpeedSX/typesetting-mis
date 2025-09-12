using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class EquipmentCapability : BaseEntity
{
    [Required]
    public Guid EquipmentId { get; set; }
    
    public Equipment? Equipment { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string CapabilityName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Value { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? Unit { get; set; }
}
