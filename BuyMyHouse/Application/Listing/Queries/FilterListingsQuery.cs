using BuyMyHouse.Application.Common.DTOs;
using MediatR;

namespace BuyMyHouse.Application.Listing.Queries;

public record FilterListingsQuery(ListingReadRequest Request) : IRequest<List<ListingReadResponse>>;
