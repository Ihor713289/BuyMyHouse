using Azure.Storage.Queues.Models;

namespace MortgageApplicationProcessor.Application.Services;

public interface IQueueReader
{
    Task<QueueMessage[]> ReadAsync(int batchSize, CancellationToken cancellationToken);
    Task DeleteMessageAsync(string messageId, string popReceipt, CancellationToken cancellationToken);

    // Moves a message that keeps failing to "{read queue}-poison", so it stops being retried but isn't lost.
    Task MoveToPoisonAsync(QueueMessage message, CancellationToken cancellationToken);
}
