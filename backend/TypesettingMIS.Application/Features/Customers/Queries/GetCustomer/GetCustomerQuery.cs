using MediatR;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Customer;

namespace TypesettingMIS.Application.Features.Customers.Queries.GetCustomer;

public record GetCustomerQuery(Guid Id) : IRequest<Result<CustomerDto>>;