using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class PricingRule : TenantEntity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string RuleType { get; set; } = string.Empty; // percentage, fixed, tiered
    
    [Required]
    public string Conditions { get; set; } = string.Empty; // JSON string
    
    [Required]
    public string Calculation { get; set; } = string.Empty; // JSON string
    
    public int Priority { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
}
