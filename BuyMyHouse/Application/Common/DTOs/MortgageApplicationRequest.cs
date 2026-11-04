namespace BuyMyHouse.Application.Common.DTOs;

// Amounts are in euros. MaritalStatus is one of: single, married, partner.
// EmploymentType is one of: employed, selfemployed, unemployed, retired.
public record MortgageApplicationRequest(
    string MaritalStatus,
    string EmploymentType,
    decimal AnnualIncome,
    decimal Savings,
    decimal Debts,
    decimal StudentLoan
    );
