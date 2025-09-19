using MediatR;
using Microsoft.EntityFrameworkCore;
using TypesettingMIS.Application.Common.Interfaces;
using TypesettingMIS.Application.Common.Models;

namespace TypesettingMIS.Application.Features.Companies.Commands.UpdateCompany;

public class UpdateCompanyCommandHandler(IApplicationDbContext context) 
    : IRequestHandler<UpdateCompanyCommand, Result>
{
    public async Task<Result> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await context.Companies
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (company == null)
        {
            return Result.Failure("Company not found");
        }

        // Update only the fields that are provided (not null)
        if (request.Name != null)
            company.Name = request.Name.Trim();

        if (request.Settings != null)
            company.Settings = request.Settings;

        if (request.SubscriptionPlan != null)
            company.SubscriptionPlan = request.SubscriptionPlan;

        if (request.IsActive.HasValue)
            company.IsActive = request.IsActive.Value;

        company.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}