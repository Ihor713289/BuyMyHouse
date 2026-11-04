namespace BuyMyHouse.Application.Common.DTOs;

// All filters are optional; omitted ones are not applied.
public record ListingReadRequest(
    string? Location,
    string? Type,
    decimal? MinPrice,
    decimal? MaxPrice
    );
