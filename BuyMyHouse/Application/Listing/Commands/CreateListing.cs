using BuyMyHouse.Application.Common.DTOs;
using MediatR;

namespace BuyMyHouse.Application.Listing.Commands;

public record CreateListing(ListingWriteRequest Request, IReadOnlyList<ImageUpload> Images) : IRequest<Guid>;
