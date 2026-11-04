using BuyMyHouse.Application.Common.DTOs;
using BuyMyHouse.Domain.Entities;
using BuyMyHouse.Domain.Enums;

namespace BuyMyHouse.Application.Common.Mappings;

public static class ListingMappings
{
    public static HousingListing ToEntity(this ListingWriteRequest request, string id, List<string> imageUrls) => new(
        HousingListing.ToPartitionKey(request.Region, id),
        id,
        request.Title.Trim(),
        request.Description.Trim(),
        (double)request.Price,
        request.SquareMeters,
        request.Region.Trim(),
        EnergyLabelExtensions.Parse(request.EnergyLabel),
        imageUrls,
        request.ZipCode.Replace(" ", "").ToUpperInvariant(),
        request.HouseNumber.Trim(),
        string.IsNullOrWhiteSpace(request.HouseNumberAddition) ? null : request.HouseNumberAddition.Trim().ToUpperInvariant(),
        PropertyTypeExtensions.Parse(request.Type));

    public static ListingReadResponse ToResponse(this HousingListing listing) => new(
        listing.RowKey,
        listing.Title,
        listing.Description,
        (decimal)listing.Price,
        listing.SquareMeters,
        listing.Region,
        listing.EnergyLabel.ToDisplayString(),
        listing.Images,
        listing.ZipCode,
        listing.HouseNumber,
        listing.HouseNumberAddition,
        listing.Type.ToDisplayString());
}
