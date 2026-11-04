namespace BuyMyHouse.Application.Common.DTOs;

public record ListingWriteRequest(
    string Title,
    string Description,
    decimal Price,
    int SquareMeters,
    string Region,
    string EnergyLabel,
    string ZipCode,
    string HouseNumber,
    string? HouseNumberAddition,
    string Type
    );
