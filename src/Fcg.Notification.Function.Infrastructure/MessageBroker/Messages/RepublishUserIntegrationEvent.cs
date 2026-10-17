namespace Fcg.Notification.Function.Infrastructure.MessageBroker.Messages
{
    public record RepublishUserIntegrationEvent(
     Guid EventId,
     Guid UserId,
     string Name,
     string Email,
     DateTime CreatedAt);
}
