using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Core.DTOs.Company;
using TypesettingMIS.Core.DTOs.Customer;
using TypesettingMIS.Core.Services;
using TypesettingMIS.Infrastructure.Data;

namespace TypesettingMIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantAwareService _tenantAwareService;

    public CustomersController(ApplicationDbContext context, ITenantContext tenantContext, ITenantAwareService tenantAwareService) 
        : base(tenantContext)
    {
        _context = context;
        _tenantAwareService = tenantAwareService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        if (!IsMultiTenant)
        {
            return BadRequest(new { message = "Tenant context is required for this operation" });
        }

        var customers = await _tenantAwareService
            .ApplyTenantFilter(_context.Customers)
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
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(Guid id)
    {
        if (!IsMultiTenant)
        {
            return BadRequest(new { message = "Tenant context is required for this operation" });
        }

        var customer = await _tenantAwareService
            .ApplyTenantFilter(_context.Customers)
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
            .FirstOrDefaultAsync();

        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createCustomerDto)
    {
        if (!IsMultiTenant)
        {
            return BadRequest(new { message = "Tenant context is required for this operation" });
        }

        var customer = new TypesettingMIS.Core.Entities.Customer
        {
            Name = createCustomerDto.Name,
            Email = createCustomerDto.Email,
            Phone = createCustomerDto.Phone,
            TaxId = createCustomerDto.TaxId,
            CompanyId = CurrentTenantId!.Value
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, UpdateCustomerDto updateCustomerDto)
    {
        if (!IsMultiTenant)
        {
            return BadRequest(new { message = "Tenant context is required for this operation" });
        }

        var customer = await _tenantAwareService
            .ApplyTenantFilter(_context.Customers)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (customer == null)
        {
            return NotFound();
        }

        if (updateCustomerDto.Name != null)
            customer.Name = updateCustomerDto.Name;
        
        if (updateCustomerDto.Email != null)
            customer.Email = updateCustomerDto.Email;
        
        if (updateCustomerDto.Phone != null)
            customer.Phone = updateCustomerDto.Phone;
        
        if (updateCustomerDto.TaxId != null)
            customer.TaxId = updateCustomerDto.TaxId;

        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        if (!IsMultiTenant)
        {
            return BadRequest(new { message = "Tenant context is required for this operation" });
        }

        var customer = await _tenantAwareService
            .ApplyTenantFilter(_context.Customers)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (customer == null)
        {
            return NotFound();
        }

        customer.IsDeleted = true;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
