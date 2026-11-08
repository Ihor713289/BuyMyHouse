namespace BuyMyHouse.Domain.Enums;

public static class EnumParsing
{
    // Case-insensitive match on member names only; rejects numeric strings like "1",
    // which Enum.TryParse would otherwise accept (even for undefined values).
    public static bool TryParseName<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
    {
        var trimmed = value?.Trim();
        if (!string.IsNullOrEmpty(trimmed) && !char.IsDigit(trimmed[0]) && trimmed[0] != '-' && trimmed[0] != '+'
            && Enum.TryParse(trimmed, ignoreCase: true, out result) && Enum.IsDefined(result))
        {
            return true;
        }

        result = default;
        return false;
    }

    public static TEnum ParseName<TEnum>(string value) where TEnum : struct, Enum =>
        TryParseName<TEnum>(value, out var result)
            ? result
            : throw new FormatException($"'{value}' is not a valid {typeof(TEnum).Name}.");

    public static IEnumerable<string> DisplayNames<TEnum>() where TEnum : struct, Enum =>
        Enum.GetNames<TEnum>().Select(name => name.ToLowerInvariant());
}
