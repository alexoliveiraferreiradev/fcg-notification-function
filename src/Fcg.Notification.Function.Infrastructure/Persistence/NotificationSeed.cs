using Microsoft.EntityFrameworkCore;

namespace Fcg.Notification.Function.Infrastructure.Persistence
{
    public static class NotificationSeed
    {
        public static async Task ApplyMigrationsAsync(NotificationDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            await context.Database.MigrateAsync();
        }
    }
}
