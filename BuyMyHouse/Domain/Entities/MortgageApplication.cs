using Azure;
using Azure.Data.Tables;
using BuyMyHouse.Domain.Enums;

namespace BuyMyHouse.Domain.Entities;

public class MortgageApplication : ITableEntity
{
    public MortgageApplication() { }

    public MortgageApplication(string userId, string id, MaritalStatus maritalStatus, EmploymentType employmentType, double annualIncome, double savings, double debts, double studentLoan)
    {
        PartitionKey = ToPartitionKey(userId);
        RowKey = id;
        MaritalStatus = maritalStatus;
        EmploymentType = employmentType;
        AnnualIncome = annualIncome;
        Savings = savings;
        Debts = debts;
        StudentLoan = studentLoan;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string PartitionKey { get; set; } = "";
    public string RowKey { get; set; } = "";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public Status Status { get; set; } = Status.Pending;
    public double AnnualIncome { get; set; }
    public double Savings { get; set; }
    public double Debts { get; set; }
    public double StudentLoan { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? OfferUrl { get; set; }

    public static string ToPartitionKey(string userId) => userId;
}
