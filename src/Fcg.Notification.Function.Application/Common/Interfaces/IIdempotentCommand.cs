namespace Fcg.Notification.Function.Application.Common.Interfaces
{
    public interface IIdempotentCommand
    {
        Guid EventId { get; }
    }
}
