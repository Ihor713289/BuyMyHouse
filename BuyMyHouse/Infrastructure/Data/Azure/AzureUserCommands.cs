using Azure;
using Azure.Data.Tables;
using BuyMyHouse.Application.Common.Services;
using BuyMyHouse.Domain.Entities;

namespace BuyMyHouse.Infrastructure.Data.Azure;

public class AzureUserCommands([FromKeyedServices(TableNames.Users)] TableClient tableClient) : IUserCommands
{
    public async Task<bool> TryCreateUser(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            // AddEntity fails with 409 when the PartitionKey/RowKey (i.e. the email) is already taken.
            await tableClient.AddEntityAsync(user, cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == StatusCodes.Status409Conflict)
        {
            return false;
        }
    }
}
