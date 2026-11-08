using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MortgageApplicationProcessor.Application.Services;
using MortgageApplicationProcessor.Infrastructure.Data.Azure;
using MortgageApplicationProcessor.Infrastructure.Options;
using MortgageApplicationProcessor.Infrastructure.Pdf;
using MortgageApplicationProcessor.Infrastructure.Queues.Azure;
using QuestPDF.Infrastructure;

// QuestPDF refuses to render without a license choice. Community is free for companies under $1M annual revenue.
QuestPDF.Settings.License = LicenseType.Community;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.Services.AddOptions<QueueOptions>().Bind(builder.Configuration.GetSection(QueueOptions.SectionName))
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(options.Read) &&
            !string.IsNullOrWhiteSpace(options.Write),
        "Queue names must be configured.")
    .ValidateOnStart();
builder.Services.AddOptions<TableOptions>().Bind(builder.Configuration.GetSection(TableOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Storage:Tables:ConnectionString must be configured.")
    .ValidateOnStart();
builder.Services.AddOptions<BlobOptions>().Bind(builder.Configuration.GetSection(BlobOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Storage:Blobs:ConnectionString must be configured.")
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IQueueReader, AzureQueueReader>();
builder.Services.AddSingleton<IQueuePublisher, AzureQueuePublisher>();
builder.Services.AddSingleton<IMortgageApplicationRepository, AzureMortgageApplicationRepository>();
builder.Services.AddSingleton<IOfferStorage, AzureBlobOfferStorage>();
builder.Services.AddSingleton<IOfferDocumentGenerator, QuestPdfOfferDocumentGenerator>();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}
builder.Build().Run();
