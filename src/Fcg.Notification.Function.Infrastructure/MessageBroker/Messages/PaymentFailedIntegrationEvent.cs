namespace Fcg.Notification.Function.Infrastructure.MessageBroker.Messages
{
    public record PaymentFailedIntegrationEvent(
        Guid EventId,
        Guid OrderId,
        Guid UserId,
        string Reason);
}
