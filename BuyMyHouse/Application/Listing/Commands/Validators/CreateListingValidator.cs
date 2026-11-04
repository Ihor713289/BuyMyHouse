using BuyMyHouse.Application.Common.DTOs;
using BuyMyHouse.Domain.Enums;
using FluentValidation;

namespace BuyMyHouse.Application.Listing.Commands.Validators;

public class CreateListingValidator : AbstractValidator<CreateListing>
{
    public CreateListingValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Request).NotNull();

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Request.Description)
                .NotEmpty()
                .MaximumLength(4000);

            RuleFor(x => x.Request.Price)
                .GreaterThan(0)
                .LessThanOrEqualTo(100_000_000);

            RuleFor(x => x.Request.SquareMeters)
                .GreaterThan(0)
                .LessThanOrEqualTo(100_000);

            RuleFor(x => x.Request.Region)
                .NotEmpty()
                .MaximumLength(100)
                // Region becomes the PartitionKey, which may not contain these characters.
                .Matches(@"^[^/\\#?\t\n\r]+$").WithMessage("'Region' contains characters that are not allowed.");

            RuleFor(x => x.Request.EnergyLabel)
                .NotEmpty()
                .Must(label => EnergyLabelExtensions.TryParse(label, out _))
                .WithMessage($"'Energy Label' must be one of: {string.Join(", ", EnergyLabelExtensions.DisplayStrings)}.");

            RuleFor(x => x.Images)
                .NotNull()
                .Must(images => images.Count <= ImageUpload.MaxCount).WithMessage($"A listing can have at most {ImageUpload.MaxCount} images.");

            RuleForEach(x => x.Images).ChildRules(image =>
            {
                image.RuleFor(i => i.Length)
                    .GreaterThan(0).WithMessage("Images may not be empty.")
                    .LessThanOrEqualTo(ImageUpload.MaxBytes).WithMessage($"Images may be at most {ImageUpload.MaxBytes / (1024 * 1024)} MB.");

                image.RuleFor(i => i.ContentType)
                    .Must(contentType => ImageUpload.ExtensionsByContentType.ContainsKey(contentType ?? ""))
                    .WithMessage($"Images must be one of: {string.Join(", ", ImageUpload.ExtensionsByContentType.Keys)}.");
            });

            RuleFor(x => x.Request.ZipCode)
                .NotEmpty()
                .Matches(@"^[1-9][0-9]{3}\s?[A-Za-z]{2}$").WithMessage("'Zip Code' must be a Dutch postcode, e.g. 1011 AB.");

            RuleFor(x => x.Request.HouseNumber)
                .NotEmpty()
                .Matches(@"^[1-9][0-9]{0,4}$").WithMessage("'House Number' must be a positive number.");

            RuleFor(x => x.Request.HouseNumberAddition)
                .Matches(@"^[A-Za-z0-9]{1,4}$").WithMessage("'House Number Addition' must be 1-4 letters or digits.")
                .When(x => !string.IsNullOrEmpty(x.Request.HouseNumberAddition));

            RuleFor(x => x.Request.Type)
                .NotEmpty()
                .Must(type => PropertyTypeExtensions.TryParse(type, out _))
                .WithMessage($"'Type' must be one of: {string.Join(", ", PropertyTypeExtensions.DisplayStrings)}.");
        });
    }
}
