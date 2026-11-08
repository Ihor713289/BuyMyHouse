using Azure.Data.Tables;
using BuyMyHouse.Domain.Entities;
using Microsoft.Extensions.Options;
using MortgageSender.Application.Services;
using MortgageSender.Infrastructure.Options;

namespace MortgageSender.Infrastructure.Data.Azure;

public class AzureUserRepository : IUserRepository
{
    private readonly TableClient _tableClient;

    public AzureUserRepository(IOptions<TableOptions> options)
    {
        _tableClient = new TableClient(options.Value.ConnectionString, options.Value.UsersTableName);
        _tableClient.CreateIfNotExists();
    }

    public async Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken)
    {
        // Users are partitioned by email (for login), so looking one up by id scans the table.
        var filter = TableClient.CreateQueryFilter($"Id eq {userId}");
        await foreach (var user in _tableClient.QueryAsync<User>(filter, maxPerPage: 1, cancellationToken: cancellationToken))
        {
            return user;
        }

        return null;
    }
}
