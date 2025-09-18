namespace TypesettingMIS.Core.DTOs.Admin;

public sealed class UserCountByCompanyDto
{
    public string CompanyName { get; init; } = string.Empty;
    public int Count { get; init; }
}