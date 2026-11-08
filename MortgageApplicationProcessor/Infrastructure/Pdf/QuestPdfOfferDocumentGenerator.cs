using System.Globalization;
using BuyMyHouse.Domain.Entities;
using BuyMyHouse.Domain.Enums;
using MortgageApplicationProcessor.Application.Offers;
using MortgageApplicationProcessor.Application.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MortgageApplicationProcessor.Infrastructure.Pdf;

public class QuestPdfOfferDocumentGenerator : IOfferDocumentGenerator
{
    // Euro amounts in Dutch notation, e.g. "€ 250.000,00".
    private static readonly CultureInfo Dutch = CultureInfo.GetCultureInfo("nl-NL");
    private static readonly Color Accent = Colors.Blue.Darken2;

    public byte[] Generate(MortgageApplication application, MortgageOffer offer) =>
        Document.Create(document => document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.8f, Unit.Centimetre);
                page.DefaultTextStyle(style => style.FontSize(9.5f));

                page.Header().Element(header => ComposeHeader(header, offer));
                page.Content().PaddingVertical(12).Column(column =>
                {
                    column.Spacing(16);
                    column.Item().Element(c => ComposeApplication(c, application, offer));
                    column.Item().Element(c => ComposeCalculation(c, offer));
                    column.Item().Element(c => ComposeRates(c, offer));
                    column.Item().Element(c => ComposeEnergyLabels(c, offer));
                    column.Item().Element(ComposeDisclaimer);
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(style => style.FontSize(8).FontColor(Colors.Grey.Darken1));
                    text.Span("BuyMyHouse mortgage offer · page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            }))
            .WithMetadata(new DocumentMetadata
            {
                Title = $"Mortgage offer {offer.ApplicationId}",
                Author = "BuyMyHouse",
                CreationDate = offer.GeneratedAt
            })
            .GeneratePdf();

    private static void ComposeHeader(IContainer container, MortgageOffer offer) =>
        container.BorderBottom(1.5f).BorderColor(Accent).PaddingBottom(8).Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("BuyMyHouse").FontSize(20).Bold().FontColor(Accent);
                column.Item().Text("Mortgage offer").FontSize(13).SemiBold();
            });
            row.RelativeItem().AlignRight().Column(column =>
            {
                column.Item().AlignRight().Text($"Application: {offer.ApplicationId}").FontSize(8);
                column.Item().AlignRight().Text($"Issued: {Date(offer.GeneratedAt)}");
                column.Item().AlignRight().Text($"Valid until: {Date(offer.ValidUntil)}").SemiBold();
            });
        });

    private static void ComposeApplication(IContainer container, MortgageApplication application, MortgageOffer offer) =>
        container.Column(column =>
        {
            column.Item().Element(c => SectionTitle(c, "1. Application details"));
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                });

                KeyValue(table, "Application date", Date(application.CreatedAt));
                KeyValue(table, "Marital status", application.MaritalStatus.ToString());
                KeyValue(table, "Employment", Humanize(application.EmploymentType));
                KeyValue(table, "Gross annual income", Money(offer.AnnualIncome));
                KeyValue(table, "Outstanding debts", Money(offer.Debts));
                KeyValue(table, "Student loan (DUO)", Money(offer.StudentLoan));
                KeyValue(table, "Savings (own funds)", Money(offer.Savings));
            });
        });

    private static void ComposeCalculation(IContainer container, MortgageOffer offer) =>
        container.Column(column =>
        {
            column.Item().Element(c => SectionTitle(c, "2. Maximum mortgage calculation"));
            column.Item().PaddingBottom(6).Text(text =>
            {
                text.Span("Formula: ").SemiBold();
                text.Span($"maximum mortgage = income × {Number(MortgageOfferPolicy.IncomeMultiplier)} " +
                          $"− debts × {Number(MortgageOfferPolicy.DebtMultiplier)} " +
                          $"− student loan × {Number(MortgageOfferPolicy.StudentLoanMultiplier)} (minimum € 0)");
            });
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                });

                HeaderRow(table, 2, "Component", "Calculation", "Amount");
                CalculationRow(table, "Income capacity",
                    $"{Money(offer.AnnualIncome)} × {Number(MortgageOfferPolicy.IncomeMultiplier)}", Money(offer.IncomeCapacity));
                CalculationRow(table, "Debts",
                    $"{Money(offer.Debts)} × {Number(MortgageOfferPolicy.DebtMultiplier)}", $"− {Money(offer.DebtDeduction)}");
                CalculationRow(table, "Student loan",
                    $"{Money(offer.StudentLoan)} × {Number(MortgageOfferPolicy.StudentLoanMultiplier)}", $"− {Money(offer.StudentLoanDeduction)}");

                table.Cell().ColumnSpan(2).Element(TotalCell).Text("Maximum mortgage (before energy label)").Bold();
                table.Cell().Element(TotalCell).AlignRight().Text(Money(offer.BaseMaximumMortgage)).Bold();
            });
            column.Item().PaddingTop(6).Text(
                    $"Indicative purchase budget: {Money(offer.BaseMaximumMortgage)} mortgage + {Money(offer.Savings)} savings = " +
                    $"{Money(offer.BaseMaximumMortgage + offer.Savings)}. Buyer's costs (kosten koper, such as transfer tax and " +
                    "notary fees) can't be financed with the mortgage and have to be paid from savings.")
                .FontSize(8.5f).FontColor(Colors.Grey.Darken2);
        });

    private static void ComposeRates(IContainer container, MortgageOffer offer) =>
        container.Column(column =>
        {
            column.Item().Element(c => SectionTitle(c, "3. Annual interest rates and monthly payments"));
            column.Item().PaddingBottom(6).Text(text =>
            {
                text.Span($"Annuity mortgage of {Money(offer.BaseMaximumMortgage)} over {offer.TermYears} years. ");
                text.Span("Monthly payment: ").SemiBold();
                text.Span($"M = P × r / (1 − (1 + r)^−n), where P is the mortgage amount, r the annual rate ÷ 12 and n = {offer.TermYears * 12} payments.");
            });
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                HeaderRow(table, 1, "Fixed-rate period", "Annual rate", "Monthly payment", "Interest, year 1", "Repayment, year 1");
                foreach (var option in offer.RateOptions)
                {
                    BodyCell(table, $"{option.FixedPeriodYears} {(option.FixedPeriodYears == 1 ? "year" : "years")}");
                    BodyCell(table, Percent(option.AnnualRatePercent), alignRight: true);
                    BodyCell(table, Money(option.MonthlyPayment), alignRight: true);
                    BodyCell(table, Money(option.FirstYearInterest), alignRight: true);
                    BodyCell(table, Money(option.MonthlyPayment * 12 - option.FirstYearInterest), alignRight: true);
                }
            });
        });

    private static void ComposeEnergyLabels(IContainer container, MortgageOffer offer) =>
        container.Column(column =>
        {
            column.Item().Element(c => SectionTitle(c, "4. Effect of the energy label of the house"));
            column.Item().PaddingBottom(6).Text(
                "Under Dutch mortgage rules (Tijdelijke regeling hypothecair krediet, Nibud 2025), you can borrow an extra " +
                "amount for an energy-efficient home on top of the income-based maximum, without an additional income test. " +
                "The label has to be registered on EP-Online. Lenders also give a sustainability discount on the interest rate. " +
                $"The table uses the {MortgageOfferPolicy.ReferenceFixedPeriodYears}-year fixed rate; " +
                "find the energy label of the house you want to buy.");
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2.2f);
                    columns.RelativeColumn(1.6f);
                    columns.RelativeColumn(1.6f);
                    columns.RelativeColumn(2);
                });

                HeaderRow(table, 1, "Label", "Extra borrowing", "Maximum mortgage", "Rate discount", "Annual rate", "Monthly payment");
                foreach (var adjustment in offer.EnergyLabelAdjustments)
                {
                    BodyCell(table, adjustment.Label.ToDisplayString(), bold: true);
                    BodyCell(table, adjustment.ExtraBorrowing > 0 ? $"+ {Money(adjustment.ExtraBorrowing)}" : "–", alignRight: true);
                    BodyCell(table, Money(adjustment.MaximumMortgage), alignRight: true);
                    BodyCell(table, adjustment.RateDiscountPercent > 0 ? $"− {Percent(adjustment.RateDiscountPercent)}" : "–", alignRight: true);
                    BodyCell(table, Percent(adjustment.AnnualRatePercent), alignRight: true);
                    BodyCell(table, Money(adjustment.MonthlyPayment), alignRight: true);
                }
            });
            column.Item().PaddingTop(6).Text(
                    "An A++++ home with an energy performance guarantee allows up to € 50.000 extra. Separately, up to € 20.000 (label E–G), " +
                    "€ 15.000 (C–D) or € 10.000 (A–B) can be added for energy-saving improvements, if they are carried out. " +
                    "Neither is included in the table above.")
                .FontSize(8.5f).FontColor(Colors.Grey.Darken2);
        });

    private static void ComposeDisclaimer(IContainer container) =>
        container.Background(Colors.Grey.Lighten4).Padding(8).Text(
                "This offer is indicative and not binding. It is based on the details you provided and on the interest rates " +
                "on the issue date. The final mortgage amount depends on verification of income and debts (including a BKR check), " +
                "the valuation of the property, and the lender's acceptance criteria.")
            .FontSize(8).FontColor(Colors.Grey.Darken2);

    private static void SectionTitle(IContainer container, string title) =>
        container.PaddingBottom(6).Text(title).FontSize(12).Bold().FontColor(Accent);

    private static void KeyValue(TableDescriptor table, string key, string value)
    {
        table.Cell().Element(BodyCellStyle).Text(key).FontColor(Colors.Grey.Darken2);
        table.Cell().Element(BodyCellStyle).Text(value).SemiBold();
    }

    private static void CalculationRow(TableDescriptor table, string component, string calculation, string amount)
    {
        BodyCell(table, component);
        BodyCell(table, calculation);
        BodyCell(table, amount, alignRight: true);
    }

    // Columns from index rightAlignFrom onwards hold amounts, so their titles are right-aligned like the values.
    private static void HeaderRow(TableDescriptor table, int rightAlignFrom, params string[] titles)
    {
        table.Header(header =>
        {
            for (var i = 0; i < titles.Length; i++)
            {
                var cell = header.Cell().Background(Accent).PaddingVertical(4).PaddingHorizontal(5);
                (i >= rightAlignFrom ? cell.AlignRight() : cell)
                    .Text(titles[i]).FontColor(Colors.White).SemiBold().FontSize(8.5f);
            }
        });
    }

    private static void BodyCell(TableDescriptor table, string text, bool alignRight = false, bool bold = false)
    {
        var cell = table.Cell().Element(BodyCellStyle);
        var span = (alignRight ? cell.AlignRight() : cell).Text(text);
        if (bold)
        {
            span.Bold();
        }
    }

    private static IContainer BodyCellStyle(IContainer container) =>
        container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(5);

    private static IContainer TotalCell(IContainer container) =>
        container.BorderTop(1).BorderColor(Colors.Grey.Darken1).Background(Colors.Grey.Lighten4).PaddingVertical(4).PaddingHorizontal(5);

    private static string Money(decimal amount) => amount.ToString("C2", Dutch);
    private static string Number(decimal value) => value.ToString("0.##", Dutch);
    private static string Percent(decimal value) => $"{value.ToString("0.00", Dutch)}%";
    private static string Date(DateTimeOffset date) => date.ToString("d MMMM yyyy", CultureInfo.InvariantCulture);

    private static string Humanize(EmploymentType type) => type switch
    {
        EmploymentType.SelfEmployed => "Self-employed",
        _ => type.ToString()
    };
}
