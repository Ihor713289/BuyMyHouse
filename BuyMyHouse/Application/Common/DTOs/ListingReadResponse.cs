namespace BuyMyHouse.Application.Common.DTOs;

public record ListingReadResponse(
    string RowKey,
    string Title,
    string Description,
    decimal Price,
    int SquareMeters,
    string Region,
    string EnergyLabel,
    List<string> Images,
    string ZipCode,
    string HouseNumber,
    string? HouseNumberAddition,
    string Type
    );
