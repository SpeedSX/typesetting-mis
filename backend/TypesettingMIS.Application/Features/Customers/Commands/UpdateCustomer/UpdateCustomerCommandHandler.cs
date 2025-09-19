using MediatR;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Application.Common.Interfaces;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    ITenantAwareService tenantAwareService) 
    : IRequestHandler<UpdateCustomerCommand, Result>
{
    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        if (!tenantContext.IsMultiTenant)
        {
            return Result.Failure("Tenant context is required for this operation");
        }

        var customer = await tenantAwareService
            .ApplyTenantFilter(context.Customers)
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            return Result.Failure("Customer not found");
        }

        // Update only the fields that are provided (not null)
        if (request.Name != null)
            customer.Name = request.Name.Trim();

        if (request.Email != null)
            customer.Email = request.Email.Trim().ToLowerInvariant();

        if (request.Phone != null)
            customer.Phone = request.Phone.Trim();

        if (request.TaxId != null)
            customer.TaxId = request.TaxId.Trim();

        customer.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}