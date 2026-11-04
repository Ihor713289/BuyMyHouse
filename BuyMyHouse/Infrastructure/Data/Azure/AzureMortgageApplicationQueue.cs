using Azure.Storage.Queues;
using BuyMyHouse.Application.Common.Services;

namespace BuyMyHouse.Infrastructure.Data.Azure;

// The QueueClient is configured with Base64 message encoding, which the Azure Functions queue trigger expects by default.
public class AzureMortgageApplicationQueue(QueueClient queueClient) : IMortgageApplicationQueue
{
    public async Task Enqueue(string applicationRowKey, CancellationToken cancellationToken = default) =>
        await queueClient.SendMessageAsync(applicationRowKey, cancellationToken);
}
