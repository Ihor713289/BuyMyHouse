using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BuyMyHouse.Application.Common.DTOs;
using BuyMyHouse.Application.Common.Services;

namespace BuyMyHouse.Infrastructure.Data.Azure;

// Images are stored as "{listingId}/{index}{extension}", so all images of a listing share a prefix.
public class AzureBlobImageStorage(BlobContainerClient containerClient) : IImageStorage
{
    public async Task<string> UploadListingImage(string listingId, int index, ImageUpload image, CancellationToken cancellationToken = default)
    {
        // The extension and Content-Type come from the validated allow-list, never from the client's file name.
        var blob = containerClient.GetBlobClient($"{listingId}/{index:D2}{ImageUpload.ExtensionsByContentType[image.ContentType]}");

        await blob.UploadAsync(image.Content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = image.ContentType.ToLowerInvariant() }
        }, cancellationToken);

        return blob.Uri.ToString();
    }

    public async Task DeleteListingImages(string listingId, CancellationToken cancellationToken = default)
    {
        await foreach (var blob in containerClient.GetBlobsAsync(new GetBlobsOptions { Prefix = $"{listingId}/" }, cancellationToken))
        {
            await containerClient.DeleteBlobIfExistsAsync(blob.Name, cancellationToken: cancellationToken);
        }
    }
}
