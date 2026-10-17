namespace Fcg.Notification.Function.Application.Ports
{
    public interface IIdempotencyService
    {
        Task<bool> TryProcessAsync(string key, CancellationToken cancellationToken);
        Task<bool> AlreadyProcessed(string key, CancellationToken cancellationToken);
        Task<int> PurgeExpiredAsync(CancellationToken cancellationToken);
    }
}
