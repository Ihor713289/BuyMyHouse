namespace BuyMyHouse.Application.Common.Services;

// Hands created applications off to the MortgageApplicationProcessor function.
public interface IMortgageApplicationQueue
{
    Task Enqueue(string applicationRowKey, CancellationToken cancellationToken = default);
}
