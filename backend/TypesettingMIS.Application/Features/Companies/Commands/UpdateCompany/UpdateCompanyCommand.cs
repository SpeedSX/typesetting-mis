using MediatR;
using TypesettingMIS.Application.Common.Models;

namespace TypesettingMIS.Application.Features.Companies.Commands.UpdateCompany;

public record UpdateCompanyCommand(
    Guid Id,
    string? Name,
    string? Settings,
    string? SubscriptionPlan,
    bool? IsActive) : IRequest<Result>;