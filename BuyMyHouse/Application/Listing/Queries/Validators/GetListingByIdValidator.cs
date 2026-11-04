using FluentValidation;

namespace BuyMyHouse.Application.Listing.Queries.Validators;

public class GetListingByIdValidator : AbstractValidator<GetListingByIdQuery>
{
    public GetListingByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _)).WithMessage("'Id' must be a valid GUID.");
    }
}
