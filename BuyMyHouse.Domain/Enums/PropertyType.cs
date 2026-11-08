namespace BuyMyHouse.Domain.Enums;

public enum PropertyType
{
    House,
    Flat,
    Studio,
    Penthouse,
    Maisonette,
    Villa,
    Bungalow,
    Farmhouse,
    Houseboat
}

public static class PropertyTypeExtensions
{
    public static string ToDisplayString(this PropertyType type) => type.ToString().ToLowerInvariant();

    public static IEnumerable<string> DisplayStrings => EnumParsing.DisplayNames<PropertyType>();

    public static PropertyType Parse(string value) => EnumParsing.ParseName<PropertyType>(value);

    public static bool TryParse(string? value, out PropertyType type) => EnumParsing.TryParseName(value, out type);
}
