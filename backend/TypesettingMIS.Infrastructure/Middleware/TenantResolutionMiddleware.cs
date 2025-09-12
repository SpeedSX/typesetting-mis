using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.Services;
using TypesettingMIS.Infrastructure.Data;

namespace TypesettingMIS.Infrastructure.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, ApplicationDbContext dbContext)
    {
        // Skip tenant resolution for certain paths
        if (ShouldSkipTenantResolution(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Try to resolve tenant from various sources
        var tenant = await ResolveTenantAsync(context, dbContext);

        if (tenant != null)
        {
            tenantContext.SetTenant(tenant);
        }

        await _next(context);
    }

    private static bool ShouldSkipTenantResolution(PathString path)
    {
        var skipPaths = new[]
        {
            "/api/health",
            "/api/seed",
            "/openapi",
            "/swagger"
        };

        return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath));
    }

    private async Task<Core.Entities.Company?> ResolveTenantAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        // 1. Try to get tenant from JWT claims (for authenticated users)
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var companyIdClaim = user.FindFirst("company_id");
            if (companyIdClaim != null && Guid.TryParse(companyIdClaim.Value, out var companyId))
            {
                return await dbContext.Companies
                    .FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted);
            }
        }

        // 2. Try to get tenant from subdomain
        var host = context.Request.Host.Host;
        if (host.Contains('.'))
        {
            var subdomain = host.Split('.')[0];
            if (subdomain != "www" && subdomain != "localhost")
            {
                return await dbContext.Companies
                    .FirstOrDefaultAsync(c => c.Domain == host && !c.IsDeleted);
            }
        }

        // 3. Try to get tenant from header
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantIdHeader))
        {
            if (Guid.TryParse(tenantIdHeader.FirstOrDefault(), out var tenantId))
            {
                return await dbContext.Companies
                    .FirstOrDefaultAsync(c => c.Id == tenantId && !c.IsDeleted);
            }
        }

        // 4. Try to get tenant from query parameter
        if (context.Request.Query.TryGetValue("tenantId", out var tenantIdQuery))
        {
            if (Guid.TryParse(tenantIdQuery.FirstOrDefault(), out var tenantId))
            {
                return await dbContext.Companies
                    .FirstOrDefaultAsync(c => c.Id == tenantId && !c.IsDeleted);
            }
        }

        return null;
    }
}
