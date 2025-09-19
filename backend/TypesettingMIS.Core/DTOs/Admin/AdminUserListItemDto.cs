namespace TypesettingMIS.Core.DTOs.Admin;

public class AdminUserListItemDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? RoleName { get; set; } = string.Empty;
}