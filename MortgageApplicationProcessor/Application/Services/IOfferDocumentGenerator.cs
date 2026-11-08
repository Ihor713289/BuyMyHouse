using BuyMyHouse.Domain.Entities;
using MortgageApplicationProcessor.Application.Offers;

namespace MortgageApplicationProcessor.Application.Services;

public interface IOfferDocumentGenerator
{
    byte[] Generate(MortgageApplication application, MortgageOffer offer);
}
