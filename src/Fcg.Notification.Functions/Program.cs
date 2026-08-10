using Fcg.Notification.Function.Application.Extensions;
using Fcg.Notification.Function.Infrastructure.Extensions;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddAplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Build().Run();
