using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.Entities;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.Infrastructure.Services;

public class TenantAwareService : ITenantAwareService
{
    private readonly ITenantContext _tenantContext;

    public TenantAwareService(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public IQueryable<T> ApplyTenantFilter<T>(IQueryable<T> query) where T : class, ITenantEntity
    {
        if (!_tenantContext.IsMultiTenant || _tenantContext.TenantId == null)
        {
            return query;
        }

        return query.Where(e => e.CompanyId == _tenantContext.TenantId.Value);
    }

    public async Task<bool> ValidateTenantAccessAsync<T>(IQueryable<T> query, Guid entityId) where T : class, ITenantEntity
    {
        if (!_tenantContext.IsMultiTenant || _tenantContext.TenantId == null)
        {
            return false;
        }

        return await query
            .Where(e => ((BaseEntity)(object)e).Id == entityId && e.CompanyId == _tenantContext.TenantId.Value)
            .AnyAsync();
    }
}
