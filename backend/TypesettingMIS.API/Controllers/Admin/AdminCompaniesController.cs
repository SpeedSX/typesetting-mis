using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.DTOs.Company;
using TypesettingMIS.Core.Services;
using TypesettingMIS.Infrastructure.Data;

namespace TypesettingMIS.API.Controllers.Admin;

[ApiController]
[Route("api/admin/companies")]
[Authorize(Roles = "Admin")]
public class AdminCompaniesController(ApplicationDbContext context, ITenantContext tenantContext)
    : BaseController(tenantContext)
{
    /// <summary>
    /// Get all companies (Admin only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies(CancellationToken cancellationToken)
    {
        var companies = await context.Companies
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Domain = c.Domain,
                Settings = c.Settings,
                SubscriptionPlan = c.SubscriptionPlan,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(companies);
    }

    /// <summary>
    /// Get company by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDto>> GetCompany(Guid id, CancellationToken cancellationToken)
    {
        var company = await context.Companies
            .AsNoTracking()
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Domain = c.Domain,
                Settings = c.Settings,
                SubscriptionPlan = c.SubscriptionPlan,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (company == null)
        {
            return NotFound();
        }

        return Ok(company);
    }

    /// <summary>
    /// Create new company (Admin only)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CompanyDto>> CreateCompany(CreateCompanyDto createCompanyDto,
        CancellationToken cancellationToken)
    {
        var normalizedName = createCompanyDto.Name.Trim();
        var normalizedDomain = createCompanyDto.Domain.Trim().ToLowerInvariant();
        var exists = await context.Companies
            .AnyAsync(c => c.Domain == normalizedDomain && !c.IsDeleted, cancellationToken);
        if (exists) return Conflict(new { message = "Domain already exists." });

        var company = new TypesettingMIS.Core.Entities.Company
        {
            Name = normalizedName,
            Domain = normalizedDomain,
            Settings = createCompanyDto.Settings,
            SubscriptionPlan = createCompanyDto.SubscriptionPlan
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync(cancellationToken);

        var companyDto = new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Domain = company.Domain,
            Settings = company.Settings,
            SubscriptionPlan = company.SubscriptionPlan,
            IsActive = company.IsActive,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt
        };

        return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, companyDto);
    }

    /// <summary>
    /// Update company (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(Guid id, UpdateCompanyDto updateCompanyDto, CancellationToken cancellationToken)
    {
        var company = await context.Companies
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (company == null)
        {
            return NotFound();
        }

        if (updateCompanyDto.Name != null)
            company.Name = updateCompanyDto.Name;
        
        if (updateCompanyDto.Settings != null)
            company.Settings = updateCompanyDto.Settings;
        
        if (updateCompanyDto.SubscriptionPlan != null)
            company.SubscriptionPlan = updateCompanyDto.SubscriptionPlan;
        
        if (updateCompanyDto.IsActive.HasValue)
            company.IsActive = updateCompanyDto.IsActive.Value;

        company.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Delete company (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(Guid id, CancellationToken cancellationToken)
    {
        var company = await context.Companies
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (company == null)
        {
            return NotFound();
        }

        company.IsDeleted = true;
        company.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
