using System.Globalization;
using Azure.Data.Tables;
using BuyMyHouse.Application.Common.DTOs;
using BuyMyHouse.Application.Common.Services;
using BuyMyHouse.Domain.Entities;
using BuyMyHouse.Domain.Enums;

namespace BuyMyHouse.Infrastructure.Data.Azure;

public class AzureListingQueries(TableClient tableClient) : IListingQueries
{
    public async Task<List<HousingListing>> FilterListings(ListingReadRequest listingReadRequest, CancellationToken cancellationToken = default)
    {
        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(listingReadRequest.Type))
        {
            // Enums are stored as their member name, e.g. "House".
            var type = PropertyTypeExtensions.Parse(listingReadRequest.Type);
            filters.Add(TableClient.CreateQueryFilter($"Type eq {type.ToString()}"));
        }

        if (listingReadRequest.MinPrice is { } minPrice)
        {
            filters.Add($"Price ge {ToODataDouble(minPrice)}");
        }

        if (listingReadRequest.MaxPrice is { } maxPrice)
        {
            filters.Add($"Price le {ToODataDouble(maxPrice)}");
        }

        if (string.IsNullOrWhiteSpace(listingReadRequest.Location))
        {
            return await Query(filters, cancellationToken);
        }

        // A region is spread over BucketCount partitions; query each one in parallel as a
        // single-partition query rather than OR-ing partition keys, which Table Storage would scan.
        var perBucket = await Task.WhenAll(HousingListing.ToPartitionKeys(listingReadRequest.Location).Select(partitionKey =>
            Query([TableClient.CreateQueryFilter($"PartitionKey eq {partitionKey}"), ..filters], cancellationToken)));

        return perBucket.SelectMany(listings => listings).ToList();
    }

    private async Task<List<HousingListing>> Query(List<string> filters, CancellationToken cancellationToken)
    {
        var filter = filters.Count == 0 ? null : string.Join(" and ", filters);

        var listings = new List<HousingListing>();
        await foreach (var listing in tableClient.QueryAsync<HousingListing>(filter, cancellationToken: cancellationToken))
        {
            listings.Add(listing);
        }

        return listings;
    }

    public async Task<HousingListing?> GetListingById(string id, CancellationToken cancellationToken = default)
    {
        // The bucket follows from the RowKey, but the region doesn't, so this scans across partitions.
        var filter = TableClient.CreateQueryFilter($"RowKey eq {id}");
        await foreach (var listing in tableClient.QueryAsync<HousingListing>(filter, maxPerPage: 1, cancellationToken: cancellationToken))
        {
            return listing;
        }

        return null;
    }

    // Price is stored as Edm.Double; the literal needs a decimal point to be compared as a double.
    private static string ToODataDouble(decimal value)
    {
        var text = ((double)value).ToString("R", CultureInfo.InvariantCulture);
        return text.Contains('.') || text.Contains('E') ? text : text + ".0";
    }
}
