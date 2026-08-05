namespace Fcg.Notification.Function.Application.Ports
{
    public interface IIdempotencyService
    {
        Task<bool> TryProcessAsync(Guid eventId);
        Task ReleaseAsync(Guid eventId);
    }
}
