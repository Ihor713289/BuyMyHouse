using Azure.Data.Tables;
using BuyMyHouse.Domain.Entities;
using Microsoft.Extensions.Options;
using MortgageApplicationProcessor.Application.Services;
using MortgageApplicationProcessor.Infrastructure.Options;

namespace MortgageApplicationProcessor.Infrastructure.Data.Azure;

public class AzureMortgageApplicationRepository : IMortgageApplicationRepository
{
    private readonly TableClient _tableClient;

    public AzureMortgageApplicationRepository(IOptions<TableOptions> options)
    {
        _tableClient = new TableClient(options.Value.ConnectionString, options.Value.MortgageApplicationsTableName);
        _tableClient.CreateIfNotExists();
    }

    public async Task<MortgageApplication?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        // Only the RowKey is known, so this is a cross-partition query.
        var filter = TableClient.CreateQueryFilter($"RowKey eq {id}");
        await foreach (var application in _tableClient.QueryAsync<MortgageApplication>(filter, maxPerPage: 1, cancellationToken: cancellationToken))
        {
            return application;
        }

        return null;
    }

    // Merge with the ETag that was read, so a concurrent change to the application isn't silently overwritten.
    public async Task UpdateAsync(MortgageApplication application, CancellationToken cancellationToken) =>
        await _tableClient.UpdateEntityAsync(application, application.ETag, TableUpdateMode.Merge, cancellationToken);
}
