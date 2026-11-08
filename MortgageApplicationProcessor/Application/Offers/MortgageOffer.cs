using BuyMyHouse.Domain.Enums;

namespace MortgageApplicationProcessor.Application.Offers;

// Every intermediate amount is kept, so the PDF can show exactly how the maximum mortgage was reached.
public sealed record MortgageOffer(
    string ApplicationId,
    DateTimeOffset GeneratedAt,
    DateTimeOffset ValidUntil,
    decimal AnnualIncome,
    decimal Debts,
    decimal StudentLoan,
    decimal Savings,
    decimal IncomeCapacity,
    decimal DebtDeduction,
    decimal StudentLoanDeduction,
    decimal BaseMaximumMortgage,
    int TermYears,
    IReadOnlyList<InterestRateOption> RateOptions,
    IReadOnlyList<EnergyLabelAdjustment> EnergyLabelAdjustments);

// Monthly annuity payment and first-year interest for the base maximum mortgage at one fixed-rate period.
public sealed record InterestRateOption(
    int FixedPeriodYears,
    decimal AnnualRatePercent,
    decimal MonthlyPayment,
    decimal FirstYearInterest);

// Effect of the energy label of the house that is bought, on the amount and on the reference rate.
public sealed record EnergyLabelAdjustment(
    EnergyLabel Label,
    decimal ExtraBorrowing,
    decimal MaximumMortgage,
    decimal RateDiscountPercent,
    decimal AnnualRatePercent,
    decimal MonthlyPayment);
