using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using MortgageApplicationProcessor.Application.Services;
using MortgageApplicationProcessor.Infrastructure.Options;

namespace MortgageApplicationProcessor.Infrastructure.Data.Azure;

public class AzureBlobOfferStorage : IOfferStorage
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobOfferStorage(IOptions<BlobOptions> options)
    {
        _containerClient = new BlobContainerClient(options.Value.ConnectionString, options.Value.OffersContainerName);
        // Private container: offers contain personal financial data. Read them with the storage credentials or a SAS.
        _containerClient.CreateIfNotExists(PublicAccessType.None);
    }

    public async Task<string> UploadAsync(string applicationId, byte[] pdf, CancellationToken cancellationToken)
    {
        // One fixed name per application, so a retry overwrites the earlier PDF instead of adding another one.
        var blob = _containerClient.GetBlobClient($"{applicationId}.pdf");

        await blob.UploadAsync(new BinaryData(pdf), new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = "application/pdf" }
        }, cancellationToken);

        return blob.Uri.ToString();
    }
}
