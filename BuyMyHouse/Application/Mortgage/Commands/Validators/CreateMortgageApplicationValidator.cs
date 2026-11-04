using BuyMyHouse.Domain.Enums;
using FluentValidation;

namespace BuyMyHouse.Application.Mortgage.Commands.Validators;

public class CreateMortgageApplicationValidator : AbstractValidator<CreateMortgageApplication>
{
    private const decimal MaxAmount = 1_000_000_000;

    public CreateMortgageApplicationValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        // Becomes the PartitionKey; set from the token, so failing this means the token is missing its subject.
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Request).NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.MaritalStatus)
                .NotEmpty()
                .Must(status => EnumParsing.TryParseName<MaritalStatus>(status, out _))
                .WithMessage($"'Marital Status' must be one of: {string.Join(", ", EnumParsing.DisplayNames<MaritalStatus>())}.");

            RuleFor(x => x.Request.EmploymentType)
                .NotEmpty()
                .Must(type => EnumParsing.TryParseName<EmploymentType>(type, out _))
                .WithMessage($"'Employment Type' must be one of: {string.Join(", ", EnumParsing.DisplayNames<EmploymentType>())}.");

            RuleFor(x => x.Request.AnnualIncome)
                .GreaterThan(0)
                .LessThanOrEqualTo(MaxAmount);

            RuleFor(x => x.Request.Savings)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(MaxAmount);

            RuleFor(x => x.Request.Debts)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(MaxAmount);

            RuleFor(x => x.Request.StudentLoan)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(MaxAmount);
        });
    }
}
