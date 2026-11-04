namespace BuyMyHouse.Application.Common.DTOs;

// An uploaded image file, decoupled from ASP.NET's IFormFile so the application layer stays web-agnostic.
public record ImageUpload(Stream Content, string ContentType, long Length)
{
    public const int MaxCount = 20;
    public const long MaxBytes = 10 * 1024 * 1024;

    // Allowed image types, with the file extension they are stored under.
    public static readonly IReadOnlyDictionary<string, string> ExtensionsByContentType = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };
}
