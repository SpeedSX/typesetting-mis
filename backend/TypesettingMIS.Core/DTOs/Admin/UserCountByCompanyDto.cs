namespace TypesettingMIS.Core.DTOs.Admin;

public sealed class UserCountByCompanyDto
{
    public required string CompanyName { get; init; }
    public int Count { get; init; }
}