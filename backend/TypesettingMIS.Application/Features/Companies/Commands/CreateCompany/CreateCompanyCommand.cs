using MediatR;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Company;

namespace TypesettingMIS.Application.Features.Companies.Commands.CreateCompany;

public record CreateCompanyCommand(
    string Name,
    string Domain,
    string? Settings,
    string SubscriptionPlan) : IRequest<Result<CompanyDto>>;