namespace Fcg.Notification.Function.Infrastructure.MessageBroker
{
    public record MassTransitEnvelope<T>(T Message);
}
