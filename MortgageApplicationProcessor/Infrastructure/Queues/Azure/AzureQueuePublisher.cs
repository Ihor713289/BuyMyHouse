using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using MortgageApplicationProcessor.Application.Services;
using MortgageApplicationProcessor.Infrastructure.Options;

namespace MortgageApplicationProcessor.Infrastructure.Queues.Azure;

public class AzureQueuePublisher : IQueuePublisher
{
    private readonly QueueClient _queueClient;

    public AzureQueuePublisher(IOptions<QueueOptions> options)
    {
        // Base64, like the web app's queue, so SendOffers (or a queue trigger with default settings) can decode it.
        _queueClient = new QueueClient(options.Value.ConnectionString, options.Value.Write,
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
        _queueClient.CreateIfNotExists();
    }

    public async Task PublishAsync(string message, CancellationToken cancellationToken = default)
    {
        await _queueClient.SendMessageAsync(message, cancellationToken);
    }
}
