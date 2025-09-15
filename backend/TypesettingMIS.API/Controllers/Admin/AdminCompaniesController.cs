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
public class AdminCompaniesController : BaseController
{
    private readonly ApplicationDbContext _context;

    public AdminCompaniesController(ApplicationDbContext context, ITenantContext tenantContext) 
        : base(tenantContext)
    {
        _context = context;
    }

    /// <summary>
    /// Get all companies (Admin only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies()
    {
        var companies = await _context.Companies
            .Where(c => !c.IsDeleted)
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
            .ToListAsync();

        return Ok(companies);
    }

    /// <summary>
    /// Get company by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDto>> GetCompany(Guid id)
    {
        var company = await _context.Companies
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
            .FirstOrDefaultAsync();

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
    public async Task<ActionResult<CompanyDto>> CreateCompany(CreateCompanyDto createCompanyDto)
    {
        var company = new TypesettingMIS.Core.Entities.Company
        {
            Name = createCompanyDto.Name,
            Domain = createCompanyDto.Domain,
            Settings = createCompanyDto.Settings,
            SubscriptionPlan = createCompanyDto.SubscriptionPlan
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

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
    public async Task<IActionResult> UpdateCompany(Guid id, UpdateCompanyDto updateCompanyDto)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

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

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Delete company (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (company == null)
        {
            return NotFound();
        }

        company.IsDeleted = true;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
