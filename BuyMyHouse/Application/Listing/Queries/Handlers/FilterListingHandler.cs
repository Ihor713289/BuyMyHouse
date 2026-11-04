using BuyMyHouse.Application.Common.DTOs;
using BuyMyHouse.Application.Common.Mappings;
using BuyMyHouse.Application.Common.Services;
using MediatR;

namespace BuyMyHouse.Application.Listing.Queries.Handlers;

public class FilterListingHandler(IListingQueries listingQueries) : IRequestHandler<FilterListingsQuery, List<ListingReadResponse>>
{
    public async Task<List<ListingReadResponse>> Handle(FilterListingsQuery request, CancellationToken cancellationToken)
    {
        var listings = await listingQueries.FilterListings(request.Request, cancellationToken);
        return listings.Select(l => l.ToResponse()).ToList();
    }
}
