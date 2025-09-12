using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.Entities;

namespace TypesettingMIS.Core.Services;

public interface ITenantAwareService
{
    IQueryable<T> ApplyTenantFilter<T>(IQueryable<T> query) where T : class, ITenantEntity;
    Task<bool> ValidateTenantAccessAsync<T>(IQueryable<T> query, Guid entityId) where T : class, ITenantEntity;
}
