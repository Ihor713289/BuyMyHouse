namespace BuyMyHouse.Domain.Enums;

// Dutch energy labels, ordered from most to least efficient.
public enum EnergyLabel
{
    APlusPlusPlusPlus,
    APlusPlusPlus,
    APlusPlus,
    APlus,
    A,
    B,
    C,
    D,
    E,
    F,
    G
}

public static class EnergyLabelExtensions
{
    // The official notation, e.g. "A++", since '+' can't appear in an enum member name.
    public static string ToDisplayString(this EnergyLabel label) => label switch
    {
        EnergyLabel.APlusPlusPlusPlus => "A++++",
        EnergyLabel.APlusPlusPlus => "A+++",
        EnergyLabel.APlusPlus => "A++",
        EnergyLabel.APlus => "A+",
        _ => label.ToString()
    };

    public static IEnumerable<string> DisplayStrings => Enum.GetValues<EnergyLabel>().Select(ToDisplayString);

    public static EnergyLabel Parse(string value) =>
        TryParse(value, out var label) ? label : throw new FormatException($"'{value}' is not a valid energy label.");

    public static bool TryParse(string? value, out EnergyLabel label)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        foreach (var candidate in Enum.GetValues<EnergyLabel>())
        {
            if (candidate.ToDisplayString() == normalized)
            {
                label = candidate;
                return true;
            }
        }

        label = default;
        return false;
    }
}
