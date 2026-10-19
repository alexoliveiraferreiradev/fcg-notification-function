using Fcg.Notification.Function.Application.Extensions;
using Fcg.Notification.Function.Infrastructure.Extensions;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development")
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Services.AddAplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();

await host.RunAsync();
