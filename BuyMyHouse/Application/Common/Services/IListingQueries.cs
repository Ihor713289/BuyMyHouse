using BuyMyHouse.Application.Common.DTOs;
using BuyMyHouse.Domain.Entities;

namespace BuyMyHouse.Application.Common.Services;

public interface IListingQueries
{
    Task<List<HousingListing>> FilterListings(ListingReadRequest listingReadRequest, CancellationToken cancellationToken = default);
    Task<HousingListing?> GetListingById(string id, CancellationToken cancellationToken = default);
}
