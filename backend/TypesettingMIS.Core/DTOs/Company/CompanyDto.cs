using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.DTOs.Company;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? Settings { get; set; }
    public string SubscriptionPlan { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCompanyDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Domain { get; set; } = string.Empty;
    
    public string? Settings { get; set; }
    
    [MaxLength(50)]
    public string SubscriptionPlan { get; set; } = "basic";
}

public class UpdateCompanyDto
{
    [MaxLength(255)]
    public string? Name { get; set; }
    
    public string? Settings { get; set; }
    
    [MaxLength(50)]
    public string? SubscriptionPlan { get; set; }
    
    public bool? IsActive { get; set; }
}
