namespace Fcg.Notification.Function.Infrastructure.Persistence
{
    internal class DatabaseSettings
    {
        public DatabaseSettings() { }
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 1433;
        public string DatabaseName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public const string DatabaseSettingsSection = "DatabaseSettings";
    }
}
