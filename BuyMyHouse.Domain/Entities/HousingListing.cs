using System.Runtime.Serialization;
using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using BuyMyHouse.Domain.Enums;

namespace BuyMyHouse.Domain.Entities;

// Azure Table Storage only persists public properties of supported EDM types
// (no decimal, char or collections), and needs a parameterless constructor to materialize entities.
public class HousingListing : ITableEntity
{
    public HousingListing() { }

    public HousingListing(string partitionKey, string rowKey, string title, string description, double price, int squareMeters, string region, EnergyLabel energyLabel, List<string> images, string zipCode, string houseNumber, string? houseNumberAddition, PropertyType type)
    {
        PartitionKey = partitionKey;
        RowKey = rowKey;
        Title = title;
        Description = description;
        Price = price;
        SquareMeters = squareMeters;
        Region = region;
        EnergyLabel = energyLabel;
        Images = images;
        ZipCode = zipCode;
        HouseNumber = houseNumber;
        HouseNumberAddition = houseNumberAddition;
        Type = type;
    }

    public string PartitionKey { get; set; } = "";
    public string RowKey { get; set; } = "";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public double Price { get; set; }
    public int SquareMeters { get; set; }
    public string Region { get; set; } = "";
    // Enums are persisted by the Tables SDK as their member name, e.g. "APlusPlus".
    public EnergyLabel EnergyLabel { get; set; }

    [IgnoreDataMember]
    public List<string> Images { get; set; } = [];

    // Persisted representation of Images, since Table Storage has no collection type.
    public string ImagesJson
    {
        get => JsonSerializer.Serialize(Images);
        set => Images = string.IsNullOrEmpty(value) ? [] : JsonSerializer.Deserialize<List<string>>(value) ?? [];
    }

    public string ZipCode { get; set; } = "";
    public string HouseNumber { get; set; } = "";
    public string? HouseNumberAddition { get; set; }
    public PropertyType Type { get; set; }

    public const int BucketCount = 16;

    public static string ToRegionSlug(string region) => region.Trim().ToLowerInvariant();

    public static string ToPartitionKey(string region, string rowKey) => ToPartitionKey(region, ToBucket(rowKey));

    public static string ToPartitionKey(string region, int bucket) => $"{ToRegionSlug(region)}-{bucket:D2}";

    // Every partition key a region's listings can live in.
    public static IEnumerable<string> ToPartitionKeys(string region) =>
        Enumerable.Range(0, BucketCount).Select(bucket => ToPartitionKey(region, bucket));

    // The RowKey is a random GUID, so its first hex digit (0-f) is uniformly distributed over the 16 buckets.
    public static int ToBucket(string rowKey) =>
        Convert.ToInt32(Guid.Parse(rowKey).ToString("N")[..1], 16) % BucketCount;
}
