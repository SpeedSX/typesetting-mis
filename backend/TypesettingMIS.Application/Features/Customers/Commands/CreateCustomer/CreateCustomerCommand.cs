using MediatR;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Customer;

namespace TypesettingMIS.Application.Features.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string Name,
    string? Email,
    string? Phone,
    string? TaxId) : IRequest<Result<CustomerDto>>;