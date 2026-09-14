using Fcg.Notification.Function.Application.Extensions;
using Fcg.Notification.Function.Infrastructure.Extensions;
using Fcg.Notification.Function.Infrastructure.MessageBroker;
using Fcg.Notification.Function.Infrastructure.Persistence;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System.Data;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddAplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await NotificationSeed.ApplyMigrationsAsync(dbContext);
}

var connection = host.Services.GetRequiredService<IConnection>();
await NotificationRabbitTopology.DeclareAllAsync(connection);
await host.RunAsync();
