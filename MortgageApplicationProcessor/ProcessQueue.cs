using Azure.Storage.Queues.Models;
using BuyMyHouse.Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MortgageApplicationProcessor.Application.Offers;
using MortgageApplicationProcessor.Application.Services;

namespace MortgageApplicationProcessor;

public class ProcessQueue(
    ILogger<ProcessQueue> logger,
    IQueueReader queueReader,
    IQueuePublisher queuePublisher,
    IMortgageApplicationRepository applications,
    IOfferDocumentGenerator documentGenerator,
    IOfferStorage offerStorage,
    TimeProvider timeProvider)
{
    // The maximum number of messages Azure Queue Storage returns per receive call.
    private const int BatchSize = 32;

    // A message that failed this many times is moved to the poison queue instead of being retried forever.
    private const int MaxDequeueCount = 5;

    [Function("ProcessQueue")]
    public async Task Run([TimerTrigger("0 0 * * *")] TimerInfo myTimer, CancellationToken cancellationToken)
    {
        logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
        await Process(cancellationToken);
        if (myTimer.ScheduleStatus is not null)
        {
            logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }

    private async Task Process(CancellationToken cancellationToken)
    {
        var processed = 0;
        var failed = 0;

        // Drain the queue. A failed message stays invisible for the visibility timeout, so it's retried
        // later in this run (if the queue isn't empty by then) or in the next one.
        while (!cancellationToken.IsCancellationRequested)
        {
            var messages = await queueReader.ReadAsync(BatchSize, cancellationToken);
            if (messages.Length == 0)
            {
                break;
            }

            foreach (var message in messages)
            {
                if (await TryProcessMessage(message, cancellationToken))
                {
                    processed++;
                }
                else
                {
                    failed++;
                }
            }
        }

        logger.LogInformation("Processed {processed} mortgage applications, {failed} failed", processed, failed);
    }

    private async Task<bool> TryProcessMessage(QueueMessage message, CancellationToken cancellationToken)
    {
        var applicationId = message.Body.ToString().Trim();

        if (message.DequeueCount > MaxDequeueCount)
        {
            logger.LogError("Mortgage application {applicationId} failed {dequeueCount} times; moving it to the poison queue",
                applicationId, message.DequeueCount);
            await queueReader.MoveToPoisonAsync(message, cancellationToken);
            return false;
        }

        try
        {
            await ProcessMessage(applicationId, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Not deleted, so the message is retried once its visibility timeout expires.
            logger.LogError(ex, "Failed to process mortgage application {applicationId} (attempt {dequeueCount})",
                applicationId, message.DequeueCount);
            return false;
        }

        await queueReader.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);
        return true;
    }

    private async Task ProcessMessage(string applicationId, CancellationToken cancellationToken)
    {
        var application = await applications.GetByIdAsync(applicationId, cancellationToken);
        if (application is null)
        {
            // The web app stores the application before enqueueing it, so it was deleted since; there's nothing to offer.
            logger.LogWarning("Mortgage application {applicationId} not found; discarding the message", applicationId);
            return;
        }

        // On a retry after the offer was already stored, only the publish step is repeated.
        if (application.Status != Status.Processed || string.IsNullOrEmpty(application.OfferUrl))
        {
            var offer = MortgageOfferCalculator.Calculate(application, timeProvider.GetUtcNow());
            var pdf = documentGenerator.Generate(application, offer);

            application.OfferUrl = await offerStorage.UploadAsync(application.RowKey, pdf, cancellationToken);
            application.Status = Status.Processed;
            await applications.UpdateAsync(application, cancellationToken);

            logger.LogInformation("Created mortgage offer for application {applicationId}: maximum mortgage {maximum}",
                applicationId, offer.BaseMaximumMortgage);
        }
        else
        {
            logger.LogInformation("Mortgage application {applicationId} already has an offer; publishing it again", applicationId);
        }

        // Published only after the OfferUrl is saved, so SendOffers always finds the PDF it has to send.
        await queuePublisher.PublishAsync(application.RowKey, cancellationToken);
    }
}
