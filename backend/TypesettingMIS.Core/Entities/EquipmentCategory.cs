using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class EquipmentCategory : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    // Navigation properties
    public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
}
