namespace MortgageApplicationProcessor.Application.Services;

public interface IQueuePublisher
{
    Task PublishAsync(string message);
}