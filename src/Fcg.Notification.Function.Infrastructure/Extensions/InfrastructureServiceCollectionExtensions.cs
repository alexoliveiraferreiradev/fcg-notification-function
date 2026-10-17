using Fcg.Core.Abstractions.Interfaces;
using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Application.Ports;
using Fcg.Notification.Function.Domain.Repositories;
using Fcg.Notification.Function.Infrastructure.Persistence;
using Fcg.Notification.Function.Infrastructure.Repository;
using Fcg.Notification.Function.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Notification.Function.Infrastructure.Extensions
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHealthCheckExtension(configuration);
            services.AddDatabaseExtension(configuration);
            services.AddScoped<IUserSnapshotRepository, UserSnapshotRepository>();   
            services.AddScoped<INotificationRepository, NotificationRepository>();           
            services.AddScoped<IEmailService, EmailService>();
            services.AddOptions<IdempotencySettings>()
                .Bind(configuration.GetSection(IdempotencySettings.IdempotencySettingsSection))
                .ValidateDataAnnotations().ValidateOnStart();
            services.AddScoped<IIdempotencyService, ProcessedMessageIdempotencyService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }


    }
}
