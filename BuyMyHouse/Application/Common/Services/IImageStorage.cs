using BuyMyHouse.Application.Common.DTOs;

namespace BuyMyHouse.Application.Common.Services;

public interface IImageStorage
{
    // Stores the image and returns the public URL it can be retrieved from.
    Task<string> UploadListingImage(string listingId, int index, ImageUpload image, CancellationToken cancellationToken = default);

    Task DeleteListingImages(string listingId, CancellationToken cancellationToken = default);
}
