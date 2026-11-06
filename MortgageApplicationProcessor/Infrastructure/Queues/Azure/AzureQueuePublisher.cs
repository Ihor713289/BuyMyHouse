using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using MortgageApplicationProcessor.Application.Services;
using MortgageApplicationProcessor.Infrastructure.Options;

namespace MortgageApplicationProcessor.Infrastructure.Queues.Azure;

public class AzureQueuePublisher: IQueuePublisher
{
    private readonly QueueClient _queueClient;

    public AzureQueuePublisher(QueueOptions options)
    {
        _queueClient = new QueueClient(options.ConnectionString, options.Write);
        _queueClient.CreateIfNotExists();
    }

    public async Task PublishAsync(string message)
    {
        await _queueClient.SendMessageAsync(message);
    }
}