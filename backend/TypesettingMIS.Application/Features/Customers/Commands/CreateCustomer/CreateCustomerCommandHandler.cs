using MediatR;
using TypesettingMIS.Application.Common.Interfaces;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Customer;
using TypesettingMIS.Core.Entities;
using TypesettingMIS.Core.Services;

namespace TypesettingMIS.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext) 
    : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        if (!tenantContext.IsMultiTenant)
        {
            return Result<CustomerDto>.Failure("Tenant context is required for this operation");
        }

        var name = request.Name.Trim();
        var email = request.Email?.Trim().ToLowerInvariant();

        var customer = new Customer
        {
            Name = name,
            Email = email,
            Phone = request.Phone?.Trim(),
            TaxId = request.TaxId?.Trim(),
            CompanyId = tenantContext.TenantId!.Value
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

        return Result<CustomerDto>.Success(customerDto);
    }
}