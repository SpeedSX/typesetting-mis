using FluentValidation;

namespace TypesettingMIS.Application.Features.Companies.Commands.CreateCompany;

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Company name is required and must be less than 200 characters");

        RuleFor(x => x.Domain)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9]*\.?[a-zA-Z]{2,}$")
            .WithMessage("Domain must be a valid domain format");

        RuleFor(x => x.SubscriptionPlan)
            .NotEmpty()
            .WithMessage("Subscription plan is required");
    }
}