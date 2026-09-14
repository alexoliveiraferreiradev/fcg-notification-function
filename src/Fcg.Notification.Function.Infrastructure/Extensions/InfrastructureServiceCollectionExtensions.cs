using Fcg.Core.Abstractions.Interfaces;
using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Application.Ports;
using Fcg.Notification.Function.Domain.Repositories;
using Fcg.Notification.Function.Infrastructure.Idempotency;
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
            services.AddRabbitMqConnectionExtension(configuration);
            services.AddCacheExtension(configuration);
            services.AddHealthCheckExtension(configuration);
            services.AddDatabaseExtension(configuration);
            services.AddScoped<IUserSnapshotRepository, UserSnapshotRepository>();   
            services.AddScoped<INotificationRepository, NotificationRepository>();           
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IIdempotencyService, RedisIdempotencyService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }


    }
}
