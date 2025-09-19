using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypesettingMIS.Application.Features.Companies.Commands.CreateCompany;
using TypesettingMIS.Application.Features.Companies.Commands.DeleteCompany;
using TypesettingMIS.Application.Features.Companies.Commands.UpdateCompany;
using TypesettingMIS.Application.Features.Companies.Queries.GetCompanies;
using TypesettingMIS.Application.Features.Companies.Queries.GetCompany;
using TypesettingMIS.Core.DTOs.Company;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.API.Controllers.Admin;

[ApiController]
[Route("api/admin/companies")]
[Authorize(Roles = "Admin")]
public class AdminCompaniesController(IMediator mediator, ITenantContext tenantContext)
    : BaseController(tenantContext)
{
    /// <summary>
    /// Get all companies (Admin only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies(CancellationToken cancellationToken)
    {
        var query = new GetCompaniesQuery();
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get company by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDto>> GetCompany(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCompanyQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create new company (Admin only)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CompanyDto>> CreateCompany(CreateCompanyDto createCompanyDto,
        CancellationToken cancellationToken)
    {
        var command = new CreateCompanyCommand(
            createCompanyDto.Name,
            createCompanyDto.Domain,
            createCompanyDto.Settings,
            createCompanyDto.SubscriptionPlan);

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(nameof(GetCompany), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update company (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(Guid id, UpdateCompanyDto updateCompanyDto, CancellationToken cancellationToken)
    {
        var command = new UpdateCompanyCommand(
            id,
            updateCompanyDto.Name,
            updateCompanyDto.Settings,
            updateCompanyDto.SubscriptionPlan,
            updateCompanyDto.IsActive);

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    /// <summary>
    /// Delete company (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCompanyCommand(id);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return NoContent();
    }
}
