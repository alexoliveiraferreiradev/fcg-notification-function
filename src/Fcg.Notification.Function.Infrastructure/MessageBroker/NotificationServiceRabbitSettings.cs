using Fcg.Core.WebApi.MessageBroker;
using System.ComponentModel.DataAnnotations;

namespace Fcg.Notification.Function.Infrastructure.MessageBroker
{
    public class NotificationServiceRabbitSettings : RabbitMqConnectionSettings
    {
        [Required(ErrorMessage ="A fila de notificação de criação de usuário é obrigatória.")]
        public string NotificationUserCreatedQueue { get; set; } = string.Empty;
        [Required(ErrorMessage ="A fila de notificação de falha no pagamento é obrigatória.")]
        public string NotificationPaymentFailedQueue { get; set; } = string.Empty;
        [Required(ErrorMessage ="A fila de notificação de processamento do pagamento é obrigatória.")]
        public string NotificationPaymentProcessedQueue { get; set; } = string.Empty;
        [Required(ErrorMessage ="A fila de notificação de falha na entrega é obrigatória.")]
        public string NotificationDeliveryFailedQueue { get; set; } = string.Empty;
    }
}
