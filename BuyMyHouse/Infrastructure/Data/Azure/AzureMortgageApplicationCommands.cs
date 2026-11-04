using Azure;
using Azure.Data.Tables;
using BuyMyHouse.Application.Common.Services;
using BuyMyHouse.Domain.Entities;

namespace BuyMyHouse.Infrastructure.Data.Azure;

public class AzureMortgageApplicationCommands([FromKeyedServices(TableNames.MortgageApplications)] TableClient tableClient)
    : IMortgageApplicationCommands
{
    public async Task CreateMortgageApplication(MortgageApplication application, CancellationToken cancellationToken = default) =>
        await tableClient.AddEntityAsync(application, cancellationToken);

    public async Task DeleteMortgageApplication(MortgageApplication application, CancellationToken cancellationToken = default) =>
        await tableClient.DeleteEntityAsync(application.PartitionKey, application.RowKey, ETag.All, cancellationToken);
}
