using BuyMyHouse.Domain.Entities;
using BuyMyHouse.Domain.Enums;

namespace MortgageApplicationProcessor.Application.Offers;

public static class MortgageOfferCalculator
{
    public static MortgageOffer Calculate(MortgageApplication application, DateTimeOffset now)
    {
        var income = ToMoney(application.AnnualIncome);
        var debts = ToMoney(application.Debts);
        var studentLoan = ToMoney(application.StudentLoan);

        // Maximum mortgage = income × 4.5 − debts × 4.5 − student loan × 1.5, never below zero.
        var incomeCapacity = income * MortgageOfferPolicy.IncomeMultiplier;
        var debtDeduction = debts * MortgageOfferPolicy.DebtMultiplier;
        var studentLoanDeduction = studentLoan * MortgageOfferPolicy.StudentLoanMultiplier;
        var baseMaximum = Math.Max(0m, incomeCapacity - debtDeduction - studentLoanDeduction);

        var rateOptions = MortgageOfferPolicy.Rates
            .Select(rate => new InterestRateOption(
                rate.FixedPeriodYears,
                rate.AnnualRatePercent,
                MonthlyAnnuity(baseMaximum, rate.AnnualRatePercent, MortgageOfferPolicy.TermYears),
                FirstYearInterest(baseMaximum, rate.AnnualRatePercent, MortgageOfferPolicy.TermYears)))
            .ToList();

        var referenceRate = MortgageOfferPolicy.Rates
            .Single(rate => rate.FixedPeriodYears == MortgageOfferPolicy.ReferenceFixedPeriodYears)
            .AnnualRatePercent;

        var labelAdjustments = Enum.GetValues<EnergyLabel>()
            .Select(label =>
            {
                // The label bonus raises an existing borrowing capacity; it doesn't create one without income.
                var extra = baseMaximum > 0 ? MortgageOfferPolicy.ExtraBorrowing(label) : 0m;
                var maximum = baseMaximum + extra;
                var discount = MortgageOfferPolicy.RateDiscountPercent(label);
                var rate = referenceRate - discount;
                return new EnergyLabelAdjustment(label, extra, maximum, discount, rate,
                    MonthlyAnnuity(maximum, rate, MortgageOfferPolicy.TermYears));
            })
            .ToList();

        return new MortgageOffer(
            application.RowKey,
            now,
            now.AddMonths(MortgageOfferPolicy.ValidityMonths),
            income,
            debts,
            studentLoan,
            ToMoney(application.Savings),
            incomeCapacity,
            debtDeduction,
            studentLoanDeduction,
            baseMaximum,
            MortgageOfferPolicy.TermYears,
            rateOptions,
            labelAdjustments);
    }

    // M = P × r / (1 − (1 + r)^−n), with r the monthly rate and n the number of monthly payments.
    public static decimal MonthlyAnnuity(decimal principal, decimal annualRatePercent, int termYears)
    {
        if (principal <= 0)
        {
            return 0m;
        }

        var months = termYears * 12;
        var monthlyRate = (double)annualRatePercent / 100 / 12;
        if (monthlyRate == 0)
        {
            return Math.Round(principal / months, 2);
        }

        var payment = (double)principal * monthlyRate / (1 - Math.Pow(1 + monthlyRate, -months));
        return Math.Round((decimal)payment, 2);
    }

    // Interest paid over the first 12 annuity payments; the rest of each payment repays the principal.
    public static decimal FirstYearInterest(decimal principal, decimal annualRatePercent, int termYears)
    {
        var payment = MonthlyAnnuity(principal, annualRatePercent, termYears);
        var monthlyRate = annualRatePercent / 100 / 12;
        var balance = principal;
        var interest = 0m;

        for (var month = 0; month < 12 && balance > 0; month++)
        {
            var monthInterest = Math.Round(balance * monthlyRate, 2);
            interest += monthInterest;
            balance -= payment - monthInterest;
        }

        return interest;
    }

    private static decimal ToMoney(double value) => Math.Round((decimal)Math.Max(0, value), 2);
}
