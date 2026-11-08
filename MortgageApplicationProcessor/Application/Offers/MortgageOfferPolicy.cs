using BuyMyHouse.Domain.Enums;

namespace MortgageApplicationProcessor.Application.Offers;

// The figures the offer is based on.
public static class MortgageOfferPolicy
{
    public const decimal IncomeMultiplier = 4.5m;
    public const decimal DebtMultiplier = 4.5m;
    public const decimal StudentLoanMultiplier = 1.5m;

    public const int TermYears = 30;

    public const int ValidityMonths = 3;

    // Indicative annual rates per fixed-rate period, in percent.
    public static readonly IReadOnlyList<(int FixedPeriodYears, decimal AnnualRatePercent)> Rates =
    [
        (1, 4.10m),
        (5, 3.80m),
        (10, 3.95m),
        (20, 4.20m),
        (30, 4.30m)
    ];

    // The fixed-rate period that the energy label table is calculated with (the most common one in the Netherlands).
    public const int ReferenceFixedPeriodYears = 10;

    // Extra borrowing capacity for the energy label of an existing home, on top of the income-based maximum
    public static decimal ExtraBorrowing(EnergyLabel label) => label switch
    {
        EnergyLabel.APlusPlusPlusPlus => 40_000m,
        EnergyLabel.APlusPlusPlus => 30_000m,
        EnergyLabel.APlusPlus or EnergyLabel.APlus => 20_000m,
        EnergyLabel.A or EnergyLabel.B => 10_000m,
        EnergyLabel.C or EnergyLabel.D => 5_000m,
        EnergyLabel.E or EnergyLabel.F or EnergyLabel.G => 0m,
        _ => throw new ArgumentOutOfRangeException(nameof(label), label, null)
    };

    public static decimal RateDiscountPercent(EnergyLabel label) => label switch
    {
        EnergyLabel.APlusPlusPlusPlus => 0.30m,
        EnergyLabel.APlusPlusPlus => 0.25m,
        EnergyLabel.APlusPlus or EnergyLabel.APlus => 0.20m,
        EnergyLabel.A => 0.15m,
        EnergyLabel.B => 0.10m,
        EnergyLabel.C => 0.05m,
        _ => 0m
    };
}
