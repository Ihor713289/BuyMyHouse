using BuyMyHouse.Application.Common.DTOs;
using BuyMyHouse.Application.Common.Exceptions;
using BuyMyHouse.Application.Common.Mappings;
using BuyMyHouse.Application.Common.Services;
using MediatR;

namespace BuyMyHouse.Application.Listing.Queries.Handlers;

public class GetListingByIdHandler(IListingQueries listingQueries) : IRequestHandler<GetListingByIdQuery, ListingReadResponse>
{
    public async Task<ListingReadResponse> Handle(GetListingByIdQuery request, CancellationToken cancellationToken)
    {
        var listing = await listingQueries.GetListingById(request.Id, cancellationToken)
                      ?? throw new NotFoundException("Listing", request.Id);
        return listing.ToResponse();
    }
}
