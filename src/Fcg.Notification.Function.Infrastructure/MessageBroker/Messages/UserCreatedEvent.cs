namespace Fcg.Notification.Function.Infrastructure.MessageBroker.Messages
{
    public record UserCreatedIntegrationEvent(
     Guid EventId,
     Guid UserId,
     string Name,
     string Email,
     DateTime CreatedAt);
}
