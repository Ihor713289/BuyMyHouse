using Azure.Data.Tables;
using BuyMyHouse.Application.Common.Services;
using BuyMyHouse.Domain.Entities;

namespace BuyMyHouse.Infrastructure.Data.Azure;

public class AzureUserQueries([FromKeyedServices(TableNames.Users)] TableClient tableClient) : IUserQueries
{
    public async Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        var response = await tableClient.GetEntityIfExistsAsync<User>(
            User.ToPartitionKey(email), User.ProfileRowKey, cancellationToken: cancellationToken);
        return response.HasValue ? response.Value : null;
    }
}
