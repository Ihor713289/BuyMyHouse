namespace BuyMyHouse.Infrastructure.Options;

public sealed class StorageOptions
{
    public StorageProviderOptions Tables { get; set; } = new();
    public StorageProviderOptions Blobs { get; set; } = new();
    public StorageProviderOptions Queues { get; set; } = new();
}

public sealed class StorageProviderOptions
{
    public string Provider { get; set; } = "";
    public string ConnectionString { get; set; } = "";
    public string? TableName { get; set; }
    public string? UsersTableName { get; set; }
    public string? MortgageApplicationsTableName { get; set; }
    public string? ContainerName { get; set; }
    public string? QueueName { get; set; }
}

public static class StorageConfiguration
{
    public const string SectionName = "Storage";
}