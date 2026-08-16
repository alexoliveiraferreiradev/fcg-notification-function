
using RabbitMQ.Client;

namespace Fcg.Notification.Function.Infrastructure.MessageBroker
{
    /// <summary>
    /// Filas consumidas pelas functions e o exchange do tipo da mensagem que as alimenta.
    /// O RabbitMQTrigger so faz declare passivo, entao a topologia precisa existir antes do host subir.
    /// Os nomes sao const para poderem ser usados no atributo [RabbitMQTrigger].
    /// </summary>
    public static class NotificationRabbitTopology
    {
        private const string ContractsNamespace = "Fcg.Core.SharedContracts.MessageContracts";

        public const string UserCreatedQueue = "notification-user-created";
        public const string PaymentProcessedQueue = "notification-payment-processed";
        public const string PaymentFailedQueue = "notification-payment-failed";
        public const string DeliveryFailedQueue = "notification-delivery-failed";

        public static readonly IReadOnlyDictionary<string, string> QueueToMessageExchange =
            new Dictionary<string, string>
            {
                [UserCreatedQueue] = $"{ContractsNamespace}:IUserCreatedIntegrationEvent",
                [PaymentProcessedQueue] = $"{ContractsNamespace}:IPaymentProcessedIntegrationEvent",
                [PaymentFailedQueue] = $"{ContractsNamespace}:IPaymentFailedIntegrationEvent",
                [DeliveryFailedQueue] = $"{ContractsNamespace}:IDeliveryFailedIntegrationEvent",
            };


        public static async Task DeclareAllAsync(IConnection connection)
        {
            await using var channel = await connection.CreateChannelAsync();

            foreach(var (queue,exchange) in QueueToMessageExchange)
            {
                var dlx = $"{queue}-dlx";
                var dlq = $"{queue}-dlq";

                await channel.ExchangeDeclareAsync(dlx, ExchangeType.Fanout, durable: true);
                await channel.QueueDeclareAsync(dlq, durable: true, exclusive: false, autoDelete: false, arguments: null);
                await channel.QueueBindAsync(dlq, dlx, routingKey: "");

                await channel.ExchangeDeclareAsync(exchange, ExchangeType.Fanout, durable: true);
                await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false,
                    arguments: new Dictionary<string, object?> { ["x-dead-letter-exchange"] = dlx });
                await channel.QueueBindAsync(queue, exchange, routingKey: "");
            }
        }
    }
}
