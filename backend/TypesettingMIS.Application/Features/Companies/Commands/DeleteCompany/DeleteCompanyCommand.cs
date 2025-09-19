using MediatR;
using TypesettingMIS.Application.Common.Models;

namespace TypesettingMIS.Application.Features.Companies.Commands.DeleteCompany;

public record DeleteCompanyCommand(Guid Id) : IRequest<Result>;