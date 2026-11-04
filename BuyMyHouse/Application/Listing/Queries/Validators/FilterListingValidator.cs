using BuyMyHouse.Domain.Enums;
using FluentValidation;

namespace BuyMyHouse.Application.Listing.Queries.Validators;

public class FilterListingValidator : AbstractValidator<FilterListingsQuery>
{
    public FilterListingValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request).NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Location)
                .MaximumLength(100);

            RuleFor(x => x.Request.Type)
                .Must(type => PropertyTypeExtensions.TryParse(type, out _))
                .WithMessage($"'Type' must be one of: {string.Join(", ", PropertyTypeExtensions.DisplayStrings)}.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Type));

            RuleFor(x => x.Request.MinPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Request.MinPrice.HasValue);

            RuleFor(x => x.Request.MaxPrice)
                .GreaterThan(0)
                .When(x => x.Request.MaxPrice.HasValue);

            RuleFor(x => x.Request.MaxPrice)
                .GreaterThanOrEqualTo(x => x.Request.MinPrice)
                .When(x => x.Request.MinPrice.HasValue && x.Request.MaxPrice.HasValue)
                .WithMessage("'Max Price' must be greater than or equal to 'Min Price'.");
        });
    }
}
