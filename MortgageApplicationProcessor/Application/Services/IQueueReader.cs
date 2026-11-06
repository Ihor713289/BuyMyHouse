using Azure;
using Azure.Storage.Queues.Models;

namespace MortgageApplicationProcessor.Application.Services;

public interface IQueueReader
{
    Task<QueueMessage[]> ReadAsync(int batchSize, CancellationToken cancellationToken);
}