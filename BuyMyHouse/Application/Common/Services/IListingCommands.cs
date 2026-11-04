using BuyMyHouse.Domain.Entities;
namespace BuyMyHouse.Application.Common.Services;

public interface IListingCommands
{
    Task<HousingListing> CreateListing(HousingListing housingListing, CancellationToken cancellationToken = default);
}
