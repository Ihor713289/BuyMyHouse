using FluentValidation;

namespace BuyMyHouse.Application.Auth.Commands.Validators;

public class LoginUserValidator : AbstractValidator<LoginUser>
{
    public LoginUserValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request).NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Email)
                .NotEmpty()
                .MaximumLength(254)
                .Matches(@"^[^/\\#?\t\n\r]+$").WithMessage("'Email' contains characters that are not allowed.");

            RuleFor(x => x.Request.Password)
                .NotEmpty()
                .MaximumLength(128);
        });
    }
}
