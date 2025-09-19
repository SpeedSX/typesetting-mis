using MediatR;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Company;

namespace TypesettingMIS.Application.Features.Companies.Queries.GetCompanies;

public record GetCompaniesQuery() : IRequest<Result<IEnumerable<CompanyDto>>>;