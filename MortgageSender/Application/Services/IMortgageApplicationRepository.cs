using BuyMyHouse.Domain.Entities;

namespace MortgageSender.Application.Services;

public interface IMortgageApplicationRepository
{
    // The id is the application's RowKey; the PartitionKey (user id) isn't part of the queue message.
    Task<MortgageApplication?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task UpdateAsync(MortgageApplication application, CancellationToken cancellationToken);
}
