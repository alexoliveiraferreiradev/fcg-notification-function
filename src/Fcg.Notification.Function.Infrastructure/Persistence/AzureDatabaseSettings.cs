using System.ComponentModel.DataAnnotations;

namespace Fcg.Notification.Function.Infrastructure.Persistence
{
    internal class AzureDatabaseSettings
    {
        public const string AzureDatabaseSettingsSection = "AzureDatabaseSettings";

        [Required(ErrorMessage = "AzureDatabaseSettings:ConnectionString é obrigatória.")]
        public string ConnectionString { get; set; } = string.Empty;
    }
}
