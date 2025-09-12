using Microsoft.AspNetCore.Http;
using TypesettingMIS.Core.Entities;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    private Company? _currentTenant;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Company? CurrentTenant => _currentTenant;

    public Guid? TenantId => _currentTenant?.Id;

    public string? TenantDomain => _currentTenant?.Domain;

    public bool IsMultiTenant => _currentTenant != null;

    public void SetTenant(Company tenant)
    {
        _currentTenant = tenant;
    }

    public void SetTenant(Guid tenantId)
    {
        // This would typically load the tenant from the database
        // For now, we'll just set the ID
        _currentTenant = new Company { Id = tenantId };
    }

    public void ClearTenant()
    {
        _currentTenant = null;
    }
}
