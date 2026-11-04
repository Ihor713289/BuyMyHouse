using System.Text;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using BuyMyHouse.Application.Common.Services;
using BuyMyHouse.Infrastructure.Data.Azure;
using BuyMyHouse.Infrastructure.Options;
using BuyMyHouse.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BuyMyHouse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddAzureTableStorage(configuration)
            .AddJwtAuthentication(configuration);

    public static IServiceCollection AddAzureTableStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageOptions>(configuration.GetSection(StorageConfiguration.SectionName));

        services.AddSingleton(sp =>
        {
            var tables = sp.GetRequiredService<IOptions<StorageOptions>>().Value.Tables;
            return new TableClient(tables.ConnectionString, tables.TableName ?? "listings");
        });
        services.AddKeyedSingleton(TableNames.Users, (sp, _) =>
        {
            var tables = sp.GetRequiredService<IOptions<StorageOptions>>().Value.Tables;
            return new TableClient(tables.ConnectionString, tables.UsersTableName ?? "users");
        });

        services.AddKeyedSingleton(TableNames.MortgageApplications, (sp, _) =>
        {
            var tables = sp.GetRequiredService<IOptions<StorageOptions>>().Value.Tables;
            return new TableClient(tables.ConnectionString, tables.MortgageApplicationsTableName ?? "mortgageapplications");
        });

        services.AddSingleton(sp =>
        {
            var queues = sp.GetRequiredService<IOptions<StorageOptions>>().Value.Queues;
            // Base64 so the Azure Functions queue trigger can decode the messages with its default settings.
            return new QueueClient(queues.ConnectionString, queues.QueueName ?? "mortgage-applications",
                new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
        });

        services.AddSingleton(sp =>
        {
            var blobs = sp.GetRequiredService<IOptions<StorageOptions>>().Value.Blobs;
            return new BlobContainerClient(blobs.ConnectionString, blobs.ContainerName ?? "listing-images");
        });

        services.AddScoped<IImageStorage, AzureBlobImageStorage>();
        services.AddScoped<IListingCommands, AzureListingCommands>();
        services.AddScoped<IListingQueries, AzureListingQueries>();
        services.AddScoped<IUserCommands, AzureUserCommands>();
        services.AddScoped<IUserQueries, AzureUserQueries>();
        services.AddScoped<IMortgageApplicationCommands, AzureMortgageApplicationCommands>();
        services.AddScoped<IMortgageApplicationQueue, AzureMortgageApplicationQueue>();

        return services;
    }

    // JWT bearer tokens issued by /api/auth/login and /api/auth/register.
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => Encoding.UTF8.GetByteCount(o.SigningKey) >= 32, "Jwt:SigningKey must be at least 32 bytes.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer) && !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Issuer and Jwt:Audience are required.")
            .Validate(o => o.ExpiryMinutes > 0, "Jwt:ExpiryMinutes must be positive.")
            .ValidateOnStart();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role
                };
            });
        services.AddAuthorization();

        return services;
    }

    // Creates the Azure tables, queue and blob container on startup if they do not exist yet.
    public static async Task InitializeStorageAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await services.GetRequiredService<TableClient>().CreateIfNotExistsAsync(cancellationToken);
        await services.GetRequiredKeyedService<TableClient>(TableNames.Users).CreateIfNotExistsAsync(cancellationToken);
        await services.GetRequiredKeyedService<TableClient>(TableNames.MortgageApplications).CreateIfNotExistsAsync(cancellationToken);
        await services.GetRequiredService<QueueClient>().CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        // Anonymous read access to individual blobs, so the image URLs stored on listings can be opened directly.
        // Requires "Allow Blob anonymous access" to be enabled on the storage account.
        await services.GetRequiredService<BlobContainerClient>().CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
    }
}
