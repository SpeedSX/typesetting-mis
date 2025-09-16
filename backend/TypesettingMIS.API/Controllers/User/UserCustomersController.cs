using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.DTOs.Customer;
using TypesettingMIS.Core.Services;
using TypesettingMIS.Infrastructure.Data;

namespace TypesettingMIS.API.Controllers.User;

[ApiController]
[Route("api/user/customers")]
[Authorize]
public class UserCustomersController(
    ApplicationDbContext context,
    ITenantContext tenantContext,
    ITenantAwareService tenantAwareService)
    : BaseController(tenantContext)
{
    /// <summary>
    /// Get all customers for the current tenant
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers(CancellationToken cancellationToken)
    {
        if (!IsMultiTenant)
        {
            return BadRequest(new { message = "Tenant context is required for this operation" });
        }

        var customers = await tenantAwareService
            .ApplyTenantFilter(context.Customers)
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                TaxId = c.TaxId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(customers);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(Guid id, CancellationToken cancellationToken)
    {
        if (!IsMultiTenant)
        {
            return BadRequest(new { message = "Tenant context is required for this operation" });
        }

        var customer = await tenantAwareService
            .ApplyTenantFilter(context.Customers)
            .AsNoTracking()
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                TaxId = c.TaxId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    /// <summary>
    /// Create new customer
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createCustomerDto, CancellationToken cancellationToken)
    {
        if (!IsMultiTenant)
        {
            return BadRequest(new { message = "Tenant context is required for this operation" });
        }

        var name = createCustomerDto.Name.Trim();
        var email = createCustomerDto.Email?.Trim().ToLowerInvariant();

        var customer = new TypesettingMIS.Core.Entities.Customer
        {
            Name = name,
            Email = email,
            Phone = createCustomerDto.Phone?.Trim(),
            TaxId = createCustomerDto.TaxId?.Trim(),
            CompanyId = CurrentTenantId!.Value
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);

        var customerDto = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            TaxId = customer.TaxId,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customerDto);
    }

    /// <summary>
    /// Update customer
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, UpdateCustomerDto updateCustomerDto, CancellationToken cancellationToken)
    {
        if (!IsMultiTenant)
        {
            return BadRequest(new { message = "Tenant context is required for this operation" });
        }

        var customer = await tenantAwareService
            .ApplyTenantFilter(context.Customers)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            return NotFound();
        }

        if (updateCustomerDto.Name != null)
            customer.Name = updateCustomerDto.Name;
        
        if (updateCustomerDto.Email != null)
            customer.Email = updateCustomerDto.Email;

        if (updateCustomerDto.Phone != null)
            customer.Phone = updateCustomerDto.Phone.Trim();

        if (updateCustomerDto.TaxId != null)
            customer.TaxId = updateCustomerDto.TaxId.Trim();

        customer.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Delete customer
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken cancellationToken)
    {
        if (!IsMultiTenant)
        {
            return BadRequest(new { message = "Tenant context is required for this operation" });
        }

        var customer = await tenantAwareService
            .ApplyTenantFilter(context.Customers)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            return NotFound();
        }

        customer.IsDeleted = true;
        customer.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
