using MediatR;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Company;

namespace TypesettingMIS.Application.Features.Companies.Queries.GetCompany;

public record GetCompanyQuery(Guid Id) : IRequest<Result<CompanyDto>>;