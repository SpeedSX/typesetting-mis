using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class RefreshToken : BaseEntity
{
    public string TokenHash { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? ReasonRevoked { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];

    // Navigation property
    public User User { get; set; } = null!;
}
