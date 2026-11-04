using BuyMyHouse.Application.Common.DTOs;
using MediatR;

namespace BuyMyHouse.Application.Listing.Queries;

public record GetListingByIdQuery(string Id) : IRequest<ListingReadResponse>;
