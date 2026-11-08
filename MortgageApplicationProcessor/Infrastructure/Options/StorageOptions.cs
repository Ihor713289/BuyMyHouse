namespace MortgageApplicationProcessor.Infrastructure.Options;

public class TableOptions
{
    public const string SectionName = "Storage:Tables";
    public string ConnectionString { get; set; } = "";
    public string MortgageApplicationsTableName { get; set; } = "mortgageapplications";
}

public class BlobOptions
{
    public const string SectionName = "Storage:Blobs";
    public string ConnectionString { get; set; } = "";
    public string OffersContainerName { get; set; } = "mortgage-offers";
}
