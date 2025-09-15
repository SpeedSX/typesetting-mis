namespace TypesettingMIS.Core.DTOs.Auth;

public class InvitationDto
{
    public string Token { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
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
