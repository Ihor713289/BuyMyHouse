using FluentValidation;

namespace BuyMyHouse.Application.Auth.Commands.Validators;

public class RegisterUserValidator : AbstractValidator<RegisterUser>
{
    public RegisterUserValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request).NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Email)
                .NotEmpty()
                .MaximumLength(254)
                .EmailAddress()
                // Email becomes the PartitionKey, which may not contain these characters.
                .Matches(@"^[^/\\#?\t\n\r]+$").WithMessage("'Email' contains characters that are not allowed.");

            RuleFor(x => x.Request.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(128)
                .Matches("[A-Z]").WithMessage("'Password' must contain an uppercase letter.")
                .Matches("[a-z]").WithMessage("'Password' must contain a lowercase letter.")
                .Matches("[0-9]").WithMessage("'Password' must contain a digit.");

            RuleFor(x => x.Request.FullName)
                .NotEmpty()
                .MaximumLength(200);
        });
    }
}
