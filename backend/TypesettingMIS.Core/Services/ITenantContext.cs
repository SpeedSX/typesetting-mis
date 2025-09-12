using TypesettingMIS.Core.Entities;

namespace TypesettingMIS.Core.Services;

public interface ITenantContext
{
    Company? CurrentTenant { get; }
    Guid? TenantId { get; }
    string? TenantDomain { get; }
    bool IsMultiTenant { get; }
    void SetTenant(Company tenant);
    void SetTenant(Guid tenantId);
    void ClearTenant();
}
