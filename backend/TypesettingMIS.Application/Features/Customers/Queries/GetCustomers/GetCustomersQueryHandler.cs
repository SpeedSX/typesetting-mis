using MediatR;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Application.Common.Interfaces;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Customer;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.Application.Features.Customers.Queries.GetCustomers;

public class GetCustomersQueryHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    ITenantAwareService tenantAwareService) 
    : IRequestHandler<GetCustomersQuery, Result<IEnumerable<CustomerDto>>>
{
    public async Task<Result<IEnumerable<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        if (!tenantContext.IsMultiTenant)
        {
            return Result<IEnumerable<CustomerDto>>.Failure("Tenant context is required for this operation");
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

        return Result<IEnumerable<CustomerDto>>.Success(customers);
    }
}