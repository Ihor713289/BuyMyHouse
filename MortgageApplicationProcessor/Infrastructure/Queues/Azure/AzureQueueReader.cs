using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Options;
using MortgageApplicationProcessor.Application.Services;
using MortgageApplicationProcessor.Infrastructure.Options;

namespace MortgageApplicationProcessor.Infrastructure.Queues.Azure;

public class AzureQueueReader : IQueueReader
{
    // Long enough to generate and upload a full batch of offers, so messages don't reappear while still being processed.
    private static readonly TimeSpan VisibilityTimeout = TimeSpan.FromMinutes(5);

    private readonly QueueClient _queueClient;
    private readonly QueueClient _poisonQueueClient;

    public AzureQueueReader(IOptions<QueueOptions> options)
    {
        // The web app enqueues Base64-encoded messages (see AzureMortgageApplicationQueue).
        var clientOptions = new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 };
        _queueClient = new QueueClient(options.Value.ConnectionString, options.Value.Read, clientOptions);
        _poisonQueueClient = new QueueClient(options.Value.ConnectionString, $"{options.Value.Read}-poison", clientOptions);
        _queueClient.CreateIfNotExists();
        _poisonQueueClient.CreateIfNotExists();
    }

    public async Task<QueueMessage[]> ReadAsync(int batchSize, CancellationToken cancellationToken)
    {
        var response = await _queueClient.ReceiveMessagesAsync(batchSize, VisibilityTimeout, cancellationToken);
        return response.Value;
    }

    public async Task DeleteMessageAsync(string messageId, string popReceipt, CancellationToken cancellationToken)
    {
        await _queueClient.DeleteMessageAsync(messageId, popReceipt, cancellationToken);
    }

    public async Task MoveToPoisonAsync(QueueMessage message, CancellationToken cancellationToken)
    {
        await _poisonQueueClient.SendMessageAsync(message.Body, cancellationToken: cancellationToken);
        await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);
    }
}
