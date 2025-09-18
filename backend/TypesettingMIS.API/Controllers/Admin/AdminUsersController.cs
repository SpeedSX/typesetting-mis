using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.DTOs.Admin;
using TypesettingMIS.Core.Services;
using TypesettingMIS.Infrastructure.Data;

namespace TypesettingMIS.API.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController(ApplicationDbContext context, ITenantContext tenantContext)
    : BaseController(tenantContext)
{
    /// <summary>
    /// Get all users across all companies (Admin only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminUserListItemDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await context.Users
            .AsNoTracking()
            .Include(u => u.Company)
            .Include(u => u.Role)
            .Select(u => new AdminUserListItemDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive,
                LastLogin = u.LastLogin,
                CompanyName = u.Company != null ? u.Company.Name : string.Empty,
                RoleName = u.Role != null ? u.Role.Name : string.Empty
            })
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    /// <summary>
    /// Get user count statistics (Admin only)
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetUserStats(CancellationToken cancellationToken)
    {
        var totalUsers = await context.Users.CountAsync(cancellationToken);
        var activeUsers = await context.Users.CountAsync(u => u.IsActive, cancellationToken);
        var usersByCompany = await context.Users
            .AsNoTracking()
            .GroupJoin(
                context.Companies.AsNoTracking(),
                u => u.CompanyId,
                c => c.Id,
                (u, cs) => new { u, companyName = cs.Select(c => c.Name).FirstOrDefault() ?? "(none)" })
            .GroupBy(x => x.companyName)
            .Select(g => new { CompanyName = g.Key, Count = g.Count() })
            .OrderBy(x => x.CompanyName)
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = totalUsers - activeUsers,
            UsersByCompany = usersByCompany
        });
    }
}
