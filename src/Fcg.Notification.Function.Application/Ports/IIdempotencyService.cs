namespace Fcg.Notification.Function.Application.Ports
{
    public interface IIdempotencyService
    {
        Task<bool> TryProcessAsync(string key);
        Task ReleaseAsync(string key);
    }
}
