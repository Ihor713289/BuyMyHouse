namespace MortgageApplicationProcessor.Infrastructure.Options;

public class QueueOptions
{
    public const string SectionName = "Storage:Queues";
    public string Read { get; set; } = "";
    public string Write { get; set; } = "";
    public string ConnectionString { get; set; } = "";
}