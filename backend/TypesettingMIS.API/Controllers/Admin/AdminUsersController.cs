using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<ActionResult<object>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await context.Users
            .Include(u => u.Company)
            .Include(u => u.Role)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.IsActive,
                u.LastLogin,
                CompanyName = u.Company.Name,
                RoleName = u.Role.Name
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
            .Include(u => u.Company)
            .GroupBy(u => u.Company.Name)
            .Select(g => new { CompanyName = g.Key, Count = g.Count() })
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
