using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MortgageApplicationProcessor;

public class ProcessQueue(ILogger<ProcessQueue> logger)
{
    [Function("ProcessQueue")]
    public void Run([TimerTrigger("0 0 * * *")] TimerInfo myTimer)
    {
        logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        if (myTimer.ScheduleStatus is not null)
        {
            logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
            
        }
    }
    
    private Task<QueueMessage> GetNextMessage()
    {
        return null;
    }
}