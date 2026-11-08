namespace MortgageApplicationProcessor.Application.Services;

public interface IOfferStorage
{
    // Stores the offer PDF (overwriting any earlier one for this application) and returns its URL.
    Task<string> UploadAsync(string applicationId, byte[] pdf, CancellationToken cancellationToken);
}
