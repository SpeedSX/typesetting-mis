using MediatR;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Application.Common.Interfaces;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.Application.Features.Customers.Commands.DeleteCustomer;

public class DeleteCustomerCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    ITenantAwareService tenantAwareService) 
    : IRequestHandler<DeleteCustomerCommand, Result>
{
    public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
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

        // Soft delete
        customer.IsDeleted = true;
        customer.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}