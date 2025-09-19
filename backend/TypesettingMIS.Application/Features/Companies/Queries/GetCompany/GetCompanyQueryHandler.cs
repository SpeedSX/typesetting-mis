using MediatR;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Application.Common.Interfaces;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Company;

namespace TypesettingMIS.Application.Features.Companies.Queries.GetCompany;

public class GetCompanyQueryHandler(IApplicationDbContext context) 
    : IRequestHandler<GetCompanyQuery, Result<CompanyDto>>
{
    public async Task<Result<CompanyDto>> Handle(GetCompanyQuery request, CancellationToken cancellationToken)
    {
        var company = await context.Companies
            .AsNoTracking()
            .Where(c => c.Id == request.Id && !c.IsDeleted)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (company == null)
        {
            return Result<CompanyDto>.Failure("Company not found");
        }

        return Result<CompanyDto>.Success(company);
    }
}