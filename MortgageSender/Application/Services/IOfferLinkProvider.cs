namespace MortgageSender.Application.Services;

public interface IOfferLinkProvider
{
    // A read-only link to the (private) offer PDF that stops working at expiresOn.
    Uri CreateTemporaryLink(string offerUrl, DateTimeOffset expiresOn);
}
