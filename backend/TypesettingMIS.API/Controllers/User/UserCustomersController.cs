using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypesettingMIS.Application.Features.Customers.Commands.CreateCustomer;
using TypesettingMIS.Application.Features.Customers.Commands.DeleteCustomer;
using TypesettingMIS.Application.Features.Customers.Commands.UpdateCustomer;
using TypesettingMIS.Application.Features.Customers.Queries.GetCustomer;
using TypesettingMIS.Application.Features.Customers.Queries.GetCustomers;
using TypesettingMIS.Core.DTOs.Customer;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.API.Controllers.User;

[ApiController]
[Route("api/user/customers")]
[Authorize]
public class UserCustomersController(IMediator mediator, ITenantContext tenantContext)
    : BaseController(tenantContext)
{
    /// <summary>
    /// Get all customers for the current tenant
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers(CancellationToken cancellationToken)
    {
        var query = new GetCustomersQuery();
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCustomerQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create new customer
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createCustomerDto, CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            createCustomerDto.Name,
            createCustomerDto.Email,
            createCustomerDto.Phone,
            createCustomerDto.TaxId);

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(nameof(GetCustomer), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update customer
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, UpdateCustomerDto updateCustomerDto, CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            updateCustomerDto.Name,
            updateCustomerDto.Email,
            updateCustomerDto.Phone,
            updateCustomerDto.TaxId);

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return NoContent();
    }

    /// <summary>
    /// Delete customer
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCustomerCommand(id);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return NoContent();
    }
}
