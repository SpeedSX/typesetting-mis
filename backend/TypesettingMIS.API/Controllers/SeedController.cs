using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.Entities;
using TypesettingMIS.Infrastructure.Data;

namespace TypesettingMIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SeedController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("data")]
    public async Task<IActionResult> SeedData()
    {
        // Check if data already exists
        if (await _context.Companies.AnyAsync())
        {
            return Ok(new { message = "Data already seeded" });
        }

        // Create a test company
        var company = new Company
        {
            Name = "Test Company",
            Domain = "testcompany.com",
            Settings = "{\"timezone\":\"UTC\",\"currency\":\"USD\"}",
            SubscriptionPlan = "basic",
            IsActive = true
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return Ok(new { 
            message = "Data seeded successfully",
            companyId = company.Id,
            companyName = company.Name
        });
    }
}
