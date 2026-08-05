using Fcg.Core.Abstractions.Common;
using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Resources;
using Fcg.Notification.Function.Domain.Enum;
using Fcg.Notification.Function.Domain.ValueObject;

namespace Fcg.Notification.Function.Domain.Entities
{
    public class NotificationMessage : EntityBase
    {        
        public EmailAddress Recipient { get; private set; }
        public NotificationType Type { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public NotificationStatus Status { get; private set; }
        public DateTime? SentAt { get; private set; }

        protected NotificationMessage()
        {
            
        }
        public NotificationMessage(EmailAddress recipient, NotificationType type)
        {
            Recipient = recipient;
            Type = type;
            CreatedAt = DateTime.UtcNow;
            Status = NotificationStatus.Pending;   

            ValidateEntity();
        }

        public void MarkAsSent()
        {
            if (Status == NotificationStatus.Sent)
                throw new DomainException("Notificação já foi marcada com enviada.");

            SentAt = DateTime.UtcNow;
            Status = NotificationStatus.Sent;
        }

       

        public (string Subject, string Body) GenerateWelcomeContent(string userName)
        {
            return (
            "Bem-vindo à FIAP Cloud Games!",
            $"Olá {userName}, a sua conta foi criada com sucesso."
            );
        }
        public (string Subject, string Body) GenerateOrderConfirmationContent(Guid orderId, string userName, DateTime date)
        {
            return (
                $"Pagamento aprovado com sucesso para o pedido {orderId}!",
                $"Olá {userName}, o seu pagamento foi aprovado com sucesso. Data Aquisição: {date}"
            );
        }

        public (string Subject, string Body) GeneratePaymentRejectionContent(Guid orderId, string userName, string reason)
        {
            return (
                $"Pagamento rejeitado para o pedido {orderId}.",
                $"Olá {userName}, infelizmente o seu pagamento foi rejeitado. Motivo: {reason}"
            );
        }


        public (string Subject, string Body) GenerateDeliveryFailedContent(Guid orderId, string userName)
        {
            return (
                $"Falha na entrega do pedido {orderId}.",
                $"Olá {userName}, infelizmente houve uma falha na entrega do seu pedido. O estorno já foi solicitado "
            );
        }

        protected override void ValidateEntity()
        {
            AssertionConcern.AssertArgumentNotNull(Recipient.Address, DomainMessages.EmailRequired);
            AssertionConcern.AssertArgumentNotNull(Type, DomainMessages.NotificationTypeRequired);
        }
    }
}
