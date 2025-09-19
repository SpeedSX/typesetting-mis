using FluentValidation;

namespace TypesettingMIS.Application.Features.Companies.Commands.UpdateCompany;

public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Company ID is required");

        When(x => !string.IsNullOrEmpty(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MaximumLength(255)
                .WithMessage("Name cannot exceed 255 characters");
        });

        When(x => !string.IsNullOrEmpty(x.SubscriptionPlan), () =>
        {
            RuleFor(x => x.SubscriptionPlan)
                .MaximumLength(50)
                .WithMessage("Subscription plan cannot exceed 50 characters");
        });
    }
}