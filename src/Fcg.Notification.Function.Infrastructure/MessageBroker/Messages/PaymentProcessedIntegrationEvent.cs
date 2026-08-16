namespace Fcg.Notification.Function.Infrastructure.MessageBroker.Messages
{
    public record PaymentProcessedIntegrationEvent(
         Guid EventId,
         Guid OrderId,
         Guid UserId,
         IEnumerable<Guid> GameIds,
         DateTime CreatedAt);
}
