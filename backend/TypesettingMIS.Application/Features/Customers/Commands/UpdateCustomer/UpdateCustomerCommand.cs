using MediatR;
using TypesettingMIS.Application.Common.Models;

namespace TypesettingMIS.Application.Features.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(
    Guid Id,
    string? Name,
    string? Email,
    string? Phone,
    string? TaxId) : IRequest<Result>;