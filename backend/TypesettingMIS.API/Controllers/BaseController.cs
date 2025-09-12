using Microsoft.AspNetCore.Mvc;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected readonly ITenantContext TenantContext;

    protected BaseController(ITenantContext tenantContext)
    {
        TenantContext = tenantContext;
    }

    protected Guid? CurrentTenantId => TenantContext.TenantId;

    protected string? CurrentTenantDomain => TenantContext.TenantDomain;

    protected bool IsMultiTenant => TenantContext.IsMultiTenant;

    protected IActionResult TenantNotFound()
    {
        return NotFound(new { message = "Tenant not found or not accessible" });
    }

    protected IActionResult TenantRequired()
    {
        return BadRequest(new { message = "Tenant context is required for this operation" });
    }
}
