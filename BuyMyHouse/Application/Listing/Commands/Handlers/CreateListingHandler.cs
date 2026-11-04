using BuyMyHouse.Application.Common.Mappings;
using BuyMyHouse.Application.Common.Services;
using MediatR;

namespace BuyMyHouse.Application.Listing.Commands.Handlers;

public class CreateListingHandler(IListingCommands listingCommands, IImageStorage imageStorage) : IRequestHandler<CreateListing, Guid>
{
    public async Task<Guid> Handle(CreateListing request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var listingId = id.ToString();

        try
        {
            var imageUrls = await Task.WhenAll(request.Images.Select((image, index) =>
                imageStorage.UploadListingImage(listingId, index, image, cancellationToken)));

            await listingCommands.CreateListing(request.Request.ToEntity(listingId, [..imageUrls]), cancellationToken);
        }
        catch
        {
            // Don't leave orphaned blobs behind when an upload or the table insert fails.
            await imageStorage.DeleteListingImages(listingId, CancellationToken.None);
            throw;
        }

        return id;
    }
}
