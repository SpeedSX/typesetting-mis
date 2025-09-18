namespace TypesettingMIS.Core.DTOs.Admin;

public sealed class UserStatsDto
{
    public int TotalUsers { get; init; }
    public int ActiveUsers { get; init; }
    public int InactiveUsers { get; init; }
    public IEnumerable<UserCountByCompanyDto> UsersByCompany { get; init; } = [];
}