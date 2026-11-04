using BuyMyHouse.Application;
using BuyMyHouse.Infrastructure;
using BuyMyHouse.Web;

// appsettings*.json live in Web/ and are copied to the output folder, so use that as the content root.
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services
    .AddWeb()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.InitializeStorageAsync();

app.UseWebPipeline();
app.Run();
