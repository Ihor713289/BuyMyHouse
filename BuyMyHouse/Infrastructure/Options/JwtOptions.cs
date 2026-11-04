namespace BuyMyHouse.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    // HMAC-SHA256 signing key; must be at least 32 bytes.
    public string SigningKey { get; set; } = "";
    public int ExpiryMinutes { get; set; } = 60;
}

// Short claim names; inbound claim mapping is disabled so these are what controllers see on User.
public static class JwtClaimTypes
{
    public const string Subject = "sub";
    public const string Name = "name";
    public const string Role = "role";
}
