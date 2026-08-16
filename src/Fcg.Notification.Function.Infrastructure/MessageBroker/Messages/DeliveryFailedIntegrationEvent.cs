namespace Fcg.Notification.Function.Infrastructure.MessageBroker.Messages
{
    public record DeliveryFailedIntegrationEvent(
        Guid EventId,
        Guid OrderId,
        Guid UserId,
        string Reason);
}
