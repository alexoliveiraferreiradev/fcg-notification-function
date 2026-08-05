using Fcg.Notification.Function.Infrastructure.Consumers;
using Fcg.Notification.Function.Infrastructure.MessageBroker;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fcg.Notification.Function.Infrastructure.Extensions
{
    internal static class MessageBrokerExtensions
    {
        public static IServiceCollection AddMessageBrokerExtension(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddOptions<RabbitMqSettings>().BindConfiguration(RabbitMqSettings.SectionName)
          .ValidateDataAnnotations().ValidateOnStart();
            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(PaymentFailedEventConsumer).Assembly);
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqConfig = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

                    cfg.Host(rabbitMqConfig.Host, rabbitMqConfig.Port, "/", h =>
                    {
                        h.Username(rabbitMqConfig.Username);
                        h.Password(rabbitMqConfig.Password);
                    });


                    cfg.UseMessageRetry(r =>
                    {
                        r.Interval(3, TimeSpan.FromSeconds(5));
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.NotificationUserCreatedQueue, e =>
                    {
                        e.ConfigureConsumer<UserCreatedEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.NotificationPaymentFailedQueue, e =>
                    {
                        e.ConfigureConsumer<PaymentFailedEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.NotificationPaymentProcessedQueue, e =>
                    {
                        e.ConfigureConsumer<PaymentProcessedEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.NotificationDeliveryFailedQueue, e =>
                    {
                        e.ConfigureConsumer<DeliveryFailedEventConsumer>(context);
                    });

                });
            });
            return services;
        }
    }
}
