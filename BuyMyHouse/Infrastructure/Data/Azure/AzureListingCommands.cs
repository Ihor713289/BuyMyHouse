using Azure.Data.Tables;
using BuyMyHouse.Application.Common.Services;
using BuyMyHouse.Domain.Entities;

namespace BuyMyHouse.Infrastructure.Data.Azure;

public class AzureListingCommands(TableClient tableClient) : IListingCommands
{
    public async Task<HousingListing> CreateListing(HousingListing housingListing, CancellationToken cancellationToken = default)
    {
        await tableClient.AddEntityAsync(housingListing, cancellationToken);
        return housingListing;
    }
}
