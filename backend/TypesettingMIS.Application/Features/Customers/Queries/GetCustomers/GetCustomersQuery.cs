using MediatR;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Customer;

namespace TypesettingMIS.Application.Features.Customers.Queries.GetCustomers;

public record GetCustomersQuery() : IRequest<Result<IEnumerable<CustomerDto>>>;