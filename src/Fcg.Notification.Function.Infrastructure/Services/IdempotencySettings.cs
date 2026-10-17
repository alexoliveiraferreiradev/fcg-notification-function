using System.ComponentModel.DataAnnotations;

namespace Fcg.Notification.Function.Infrastructure.Services
{
    internal class IdempotencySettings
    {
        public const string IdempotencySettingsSection = "Idempotency";

        /// <summary>
        /// Por quantos dias a chave de uma mensagem processada é mantida. Precisa cobrir
        /// a janela em que o Service Bus ainda pode reentregar a mensagem.
        /// </summary>
        [Range(1, 365, ErrorMessage = "Idempotency:RetentionInDays deve estar entre 1 e 365.")]
        public int RetentionInDays { get; set; } = 7;
    }
}
