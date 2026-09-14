using Fcg.Core.WebApi.MessageBroker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Fcg.Notification.Function.Infrastructure.Extensions
{
    internal static class MessageBrokerExtensions
    {
        public static IServiceCollection AddRabbitMqConnectionExtension(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddOptions<RabbitMqConnectionSettings>().BindConfiguration(RabbitMqConnectionSettings.SectionName)
          .ValidateDataAnnotations().ValidateOnStart();

            services.AddSingleton<IConnection>(sp =>
            {
                var cfg = sp.GetRequiredService<IOptions<RabbitMqConnectionSettings>>();

                var factory = new ConnectionFactory
                {
                    HostName = cfg.Value.Host,
                    Port = cfg.Value.Port,
                    UserName = cfg.Value.Username,
                    Password = cfg.Value.Password,
                    VirtualHost = "/"
                };

                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });
            return services;
        }
    }
}
