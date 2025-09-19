using MediatR;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Application.Common.Interfaces;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Customer;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.Application.Features.Customers.Queries.GetCustomer;

public class GetCustomerQueryHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    ITenantAwareService tenantAwareService) 
    : IRequestHandler<GetCustomerQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        if (!tenantContext.IsMultiTenant)
        {
            return Result<CustomerDto>.Failure("Tenant context is required for this operation");
        }

        var customer = await tenantAwareService
            .ApplyTenantFilter(context.Customers)
            .AsNoTracking()
            .Where(c => c.Id == request.Id && !c.IsDeleted)
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
            return Result<CustomerDto>.Failure("Customer not found");
        }

        return Result<CustomerDto>.Success(customer);
    }
}