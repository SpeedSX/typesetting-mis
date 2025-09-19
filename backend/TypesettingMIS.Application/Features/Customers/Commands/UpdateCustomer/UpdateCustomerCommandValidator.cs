using FluentValidation;

namespace TypesettingMIS.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        When(x => !string.IsNullOrEmpty(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MaximumLength(255)
                .WithMessage("Name cannot exceed 255 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .MaximumLength(255)
                .WithMessage("Email cannot exceed 255 characters")
                .EmailAddress()
                .WithMessage("Email must be a valid email address");
        });

        When(x => !string.IsNullOrEmpty(x.Phone), () =>
        {
            RuleFor(x => x.Phone)
                .MaximumLength(50)
                .WithMessage("Phone cannot exceed 50 characters");
        });

        When(x => !string.IsNullOrEmpty(x.TaxId), () =>
        {
            RuleFor(x => x.TaxId)
                .MaximumLength(100)
                .WithMessage("Tax ID cannot exceed 100 characters");
        });
    }
}