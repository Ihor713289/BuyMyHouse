using MailKit.Security;

namespace MortgageSender.Infrastructure.Options;

public class QueueOptions
{
    public const string SectionName = "Storage:Queues";
    public string ConnectionString { get; set; } = "";
    // The queue MortgageApplicationProcessor publishes processed application ids to.
    public string Read { get; set; } = "";
}

public class TableOptions
{
    public const string SectionName = "Storage:Tables";
    public string ConnectionString { get; set; } = "";
    public string MortgageApplicationsTableName { get; set; } = "mortgageapplications";
    public string UsersTableName { get; set; } = "users";
}

public class BlobOptions
{
    public const string SectionName = "Storage:Blobs";
    // Must contain the account key, since that's what the SAS links are signed with.
    public string ConnectionString { get; set; } = "";
}

public class SmtpOptions
{
    public const string SectionName = "Smtp";
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    // Auto, None, SslOnConnect, StartTls or StartTlsWhenAvailable.
    public SecureSocketOptions Security { get; set; } = SecureSocketOptions.Auto;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "BuyMyHouse";
}

public class OfferEmailOptions
{
    public const string SectionName = "OfferEmail";
    // How long the link in the email can be used to view the offer.
    public int LinkValidityHours { get; set; } = 24;
}
