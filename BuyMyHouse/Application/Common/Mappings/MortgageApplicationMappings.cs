using BuyMyHouse.Application.Common.DTOs;
using BuyMyHouse.Domain.Entities;
using BuyMyHouse.Domain.Enums;

namespace BuyMyHouse.Application.Common.Mappings;

public static class MortgageApplicationMappings
{
    public static MortgageApplication ToEntity(this MortgageApplicationRequest request, string userId, string id) => new(
        userId,
        id,
        EnumParsing.ParseName<MaritalStatus>(request.MaritalStatus),
        EnumParsing.ParseName<EmploymentType>(request.EmploymentType),
        (double)request.AnnualIncome,
        (double)request.Savings,
        (double)request.Debts,
        (double)request.StudentLoan);
}
