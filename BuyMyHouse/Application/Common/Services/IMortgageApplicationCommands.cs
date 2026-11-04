using BuyMyHouse.Domain.Entities;

namespace BuyMyHouse.Application.Common.Services;

public interface IMortgageApplicationCommands
{
    Task CreateMortgageApplication(MortgageApplication application, CancellationToken cancellationToken = default);

    Task DeleteMortgageApplication(MortgageApplication application, CancellationToken cancellationToken = default);
}
