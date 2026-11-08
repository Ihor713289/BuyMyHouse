using Azure.Storage.Queues.Models;
using BuyMyHouse.Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MortgageSender.Application.Offers;
using MortgageSender.Application.Services;
using MortgageSender.Infrastructure.Options;

namespace MortgageSender;

public class SendOffers(
    ILogger<SendOffers> logger,
    IQueueReader queueReader,
    IMortgageApplicationRepository applications,
    IUserRepository users,
    IOfferLinkProvider offerLinks,
    IEmailSender emailSender,
    IOptions<OfferEmailOptions> emailOptions,
    TimeProvider timeProvider)
{
    private const int BatchSize = 32;
    
    private const int MaxDequeueCount = 5;

    [Function("SendOffers")]
    public async Task Run([TimerTrigger("0 8 * * *")] TimerInfo myTimer, CancellationToken cancellationToken)
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
        var sent = 0;
        var failed = 0;
        
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
                    sent++;
                }
                else
                {
                    failed++;
                }
            }
        }

        logger.LogInformation("Handled {sent} mortgage offers, {failed} failed", sent, failed);
    }

    private async Task<bool> TryProcessMessage(QueueMessage message, CancellationToken cancellationToken)
    {
        var applicationId = message.Body.ToString().Trim();

        if (message.DequeueCount > MaxDequeueCount)
        {
            logger.LogError("Sending the offer for mortgage application {applicationId} failed {dequeueCount} times; moving it to the poison queue",
                applicationId, message.DequeueCount);
            await queueReader.MoveToPoisonAsync(message, cancellationToken);
            return false;
        }

        try
        {
            await SendOffer(applicationId, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to send the offer for mortgage application {applicationId} (attempt {dequeueCount})",
                applicationId, message.DequeueCount);
            return false;
        }

        await queueReader.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);
        return true;
    }

    private async Task SendOffer(string applicationId, CancellationToken cancellationToken)
    {
        var application = await applications.GetByIdAsync(applicationId, cancellationToken);
        if (application is null)
        {
            logger.LogWarning("Mortgage application {applicationId} not found; discarding the message", applicationId);
            return;
        }

        if (application.OfferSentAt is not null)
        {
            // The processor may publish an id twice (at-least-once); the applicant should get one email.
            logger.LogInformation("Offer for mortgage application {applicationId} was already sent at {sentAt}; skipping",
                applicationId, application.OfferSentAt);
            return;
        }

        if (application.Status != Status.Processed || string.IsNullOrEmpty(application.OfferUrl))
        {
            throw new InvalidOperationException($"Mortgage application {applicationId} has no offer yet.");
        }

        var userId = application.PartitionKey;
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            logger.LogWarning("User {userId} of mortgage application {applicationId} not found or has no email; discarding the message",
                userId, applicationId);
            return;
        }

        var now = timeProvider.GetUtcNow();
        var linkExpiresOn = now.AddHours(emailOptions.Value.LinkValidityHours);
        var link = offerLinks.CreateTemporaryLink(application.OfferUrl, linkExpiresOn);

        await emailSender.SendAsync(OfferEmailComposer.Compose(user, application, link, linkExpiresOn), cancellationToken);

        application.OfferSentAt = now;
        await applications.UpdateAsync(application, cancellationToken);

        logger.LogInformation("Sent the offer for mortgage application {applicationId}; the link expires at {expiresOn}",
            applicationId, linkExpiresOn);
    }
}
