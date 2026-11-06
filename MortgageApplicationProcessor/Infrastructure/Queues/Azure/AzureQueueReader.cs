using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using MortgageApplicationProcessor.Application.Services;
using MortgageApplicationProcessor.Infrastructure.Options;

namespace MortgageApplicationProcessor.Infrastructure.Queues.Azure;

public class AzureQueueReader: IQueueReader
{
    private readonly QueueClient _queueClient;
    public AzureQueueReader(QueueOptions options)
    {
        _queueClient = new QueueClient(options.ConnectionString, options.Read);
        _queueClient.CreateIfNotExists();
    }
    
    public async Task<QueueMessage[]> ReadAsync(int batchSize, CancellationToken cancellationToken)
    {
        var response = await _queueClient.ReceiveMessagesAsync(maxMessages: batchSize, cancellationToken: cancellationToken);
        return response.Value;
    }
}