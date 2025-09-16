namespace TypesettingMIS.Core.DTOs.Auth;

public class InvitationDto
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public Guid? UsedByUserId { get; set; }
    public string? UsedByEmail { get; set; }
}

public class CreateInvitationDto
{
    public Guid CompanyId { get; set; }
    public int ExpirationHours { get; set; } = 24; // Default 24 hours
}

public class ValidateInvitationDto
{
    public string Token { get; set; } = string.Empty;
}
