using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MortgageApplicationProcessor.Infrastructure.Options;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.Services.AddOptions<QueueOptions>().Bind(builder.Configuration.GetSection(QueueOptions.SectionName))
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(options.Read) &&
            !string.IsNullOrWhiteSpace(options.Write),
        "Queue names must be configured.")
    .ValidateOnStart();
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}
builder.Build().Run();