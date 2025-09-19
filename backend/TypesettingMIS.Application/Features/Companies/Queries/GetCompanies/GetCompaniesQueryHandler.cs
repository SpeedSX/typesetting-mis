using MediatR;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Application.Common.Interfaces;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Company;

namespace TypesettingMIS.Application.Features.Companies.Queries.GetCompanies;

public class GetCompaniesQueryHandler(IApplicationDbContext context) 
    : IRequestHandler<GetCompaniesQuery, Result<IEnumerable<CompanyDto>>>
{
    public async Task<Result<IEnumerable<CompanyDto>>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companies = await context.Companies
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Domain = c.Domain,
                Settings = c.Settings,
                SubscriptionPlan = c.SubscriptionPlan,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<CompanyDto>>.Success(companies);
    }
}