using MediatR;
using TypesettingMIS.Application.Common.Models;

namespace TypesettingMIS.Application.Features.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(Guid Id) : IRequest<Result>;