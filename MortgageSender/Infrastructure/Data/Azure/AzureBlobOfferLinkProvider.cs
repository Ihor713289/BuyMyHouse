using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using MortgageSender.Application.Services;
using MortgageSender.Infrastructure.Options;

namespace MortgageSender.Infrastructure.Data.Azure;

public class AzureBlobOfferLinkProvider(IOptions<BlobOptions> options) : IOfferLinkProvider
{
    private readonly BlobServiceClient _serviceClient = new(options.Value.ConnectionString);

    public Uri CreateTemporaryLink(string offerUrl, DateTimeOffset expiresOn)
    {
        // Re-create the blob client from the stored URL's container and name, using the account-key credentials.
        var location = new BlobUriBuilder(new Uri(offerUrl));
        var blob = _serviceClient.GetBlobContainerClient(location.BlobContainerName).GetBlobClient(location.BlobName);

        if (!blob.CanGenerateSasUri)
        {
            throw new InvalidOperationException(
                "Storage:Blobs:ConnectionString has no account key, so no SAS link can be signed for the offer.");
        }

        var sas = new BlobSasBuilder(BlobSasPermissions.Read, expiresOn)
        {
            BlobContainerName = blob.BlobContainerName,
            BlobName = blob.Name,
            Resource = "b",
            // Allow for clock skew between this machine and the storage service.
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            // Open the PDF in the browser instead of downloading it. No quotes around the filename: they'd end up
            // unencoded in the URL, and plain-text mail clients stop auto-linking at a '"'.
            ContentDisposition = "inline; filename=mortgage-offer.pdf",
            ContentType = "application/pdf"
        };

        return blob.GenerateSasUri(sas);
    }
}
