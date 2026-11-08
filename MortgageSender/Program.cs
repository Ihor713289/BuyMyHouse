using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MortgageSender.Application.Services;
using MortgageSender.Infrastructure.Data.Azure;
using MortgageSender.Infrastructure.Email;
using MortgageSender.Infrastructure.Options;
using MortgageSender.Infrastructure.Queues.Azure;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddOptions<QueueOptions>().Bind(builder.Configuration.GetSection(QueueOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString) && !string.IsNullOrWhiteSpace(options.Read),
        "Storage:Queues:ConnectionString and Storage:Queues:Read must be configured.")
    .ValidateOnStart();
builder.Services.AddOptions<TableOptions>().Bind(builder.Configuration.GetSection(TableOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Storage:Tables:ConnectionString must be configured.")
    .ValidateOnStart();
builder.Services.AddOptions<BlobOptions>().Bind(builder.Configuration.GetSection(BlobOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Storage:Blobs:ConnectionString must be configured.")
    .ValidateOnStart();
builder.Services.AddOptions<SmtpOptions>().Bind(builder.Configuration.GetSection(SmtpOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Host) && !string.IsNullOrWhiteSpace(options.FromAddress),
        "Smtp:Host and Smtp:FromAddress must be configured.")
    .ValidateOnStart();
builder.Services.AddOptions<OfferEmailOptions>().Bind(builder.Configuration.GetSection(OfferEmailOptions.SectionName))
    .Validate(options => options.LinkValidityHours > 0, "OfferEmail:LinkValidityHours must be positive.")
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IQueueReader, AzureQueueReader>();
builder.Services.AddSingleton<IMortgageApplicationRepository, AzureMortgageApplicationRepository>();
builder.Services.AddSingleton<IUserRepository, AzureUserRepository>();
builder.Services.AddSingleton<IOfferLinkProvider, AzureBlobOfferLinkProvider>();
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
