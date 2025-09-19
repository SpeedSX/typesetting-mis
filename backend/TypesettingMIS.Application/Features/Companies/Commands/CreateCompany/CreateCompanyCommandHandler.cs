using MediatR;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Application.Common.Interfaces;
using TypesettingMIS.Application.Common.Models;
using TypesettingMIS.Core.DTOs.Company;
using TypesettingMIS.Core.Entities;

namespace TypesettingMIS.Application.Features.Companies.Commands.CreateCompany;

public class CreateCompanyCommandHandler(IApplicationDbContext context) 
    : IRequestHandler<CreateCompanyCommand, Result<CompanyDto>>
{
    public async Task<Result<CompanyDto>> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();
        var normalizedDomain = request.Domain.Trim().ToLowerInvariant();
        
        var exists = await context.Companies
            .AnyAsync(c => c.Domain == normalizedDomain && !c.IsDeleted, cancellationToken);
        
        if (exists) 
            return Result<CompanyDto>.Failure("Domain already exists");

        var company = new Company
        {
            Name = normalizedName,
            Domain = normalizedDomain,
            Settings = request.Settings,
            SubscriptionPlan = request.SubscriptionPlan
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync(cancellationToken);

        var companyDto = new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Domain = company.Domain,
            Settings = company.Settings,
            SubscriptionPlan = company.SubscriptionPlan,
            IsActive = company.IsActive,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt
        };

        return Result<CompanyDto>.Success(companyDto);
    }
}