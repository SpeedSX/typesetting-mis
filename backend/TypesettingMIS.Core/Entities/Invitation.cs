namespace TypesettingMIS.Core.Entities;

public class Invitation : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public Guid? UsedByUserId { get; set; }
    public string? UsedByEmail { get; set; }

    // Navigation property
    public Company Company { get; set; } = null!;
}
