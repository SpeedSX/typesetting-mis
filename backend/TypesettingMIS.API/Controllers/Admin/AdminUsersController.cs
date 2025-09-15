using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.Services;
using TypesettingMIS.Infrastructure.Data;

namespace TypesettingMIS.API.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : BaseController
{
    private readonly ApplicationDbContext _context;

    public AdminUsersController(ApplicationDbContext context, ITenantContext tenantContext) 
        : base(tenantContext)
    {
        _context = context;
    }

    /// <summary>
    /// Get all users across all companies (Admin only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetUsers()
    {
        var users = await _context.Users
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
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Get user count statistics (Admin only)
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetUserStats()
    {
        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
        var usersByCompany = await _context.Users
            .Include(u => u.Company)
            .GroupBy(u => u.Company.Name)
            .Select(g => new { CompanyName = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(new
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = totalUsers - activeUsers,
            UsersByCompany = usersByCompany
        });
    }
}
