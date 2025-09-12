using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public interface ITenantEntity
{
    Guid CompanyId { get; set; }
}

public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; } = false;
}

public abstract class TenantEntity : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    
    public Company? Company { get; set; }
}
