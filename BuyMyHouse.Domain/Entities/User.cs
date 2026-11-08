using Azure;
using Azure.Data.Tables;

namespace BuyMyHouse.Domain.Entities;

public class User : ITableEntity
{
    public const string ProfileRowKey = "profile";

    public User() { }

    public User(string id, string email, string fullName, string passwordHash, string role)
    {
        PartitionKey = ToPartitionKey(email);
        RowKey = ProfileRowKey;
        Id = id;
        Email = email.Trim();
        FullName = fullName;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string PartitionKey { get; set; } = "";
    public string RowKey { get; set; } = "";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }

    public static string ToPartitionKey(string email) => email.Trim().ToLowerInvariant();
}
