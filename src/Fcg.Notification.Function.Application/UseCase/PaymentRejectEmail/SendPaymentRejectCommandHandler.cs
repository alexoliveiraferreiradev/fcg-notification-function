using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Domain.Entities;
using Fcg.Notification.Function.Domain.Enum;
using Fcg.Notification.Function.Domain.Repositories;
using Fcg.Notification.Function.Domain.ValueObject;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Notification.Function.Application.UseCase.PaymentRejectEmail
{
    public class SendPaymentRejectCommandHandler : IRequestHandler<SendPaymentRejectCommand>
    {
        private readonly IEmailService _emailService;
        private readonly IUserSnapshotRepository _userSnapshotRepository;
        private readonly ILogger<SendPaymentRejectCommandHandler> _logger;

        public SendPaymentRejectCommandHandler(IEmailService emailService,
             IUserSnapshotRepository userSnapshotRepository, ILogger<SendPaymentRejectCommandHandler> logger)
        {
            _emailService = emailService;
            _userSnapshotRepository = userSnapshotRepository;
            _logger = logger;
        }

        public async Task Handle(SendPaymentRejectCommand command, CancellationToken cancellationToken)
        {
            var userProfile = await _userSnapshotRepository.GetByUserIdAsync(command.UserId, cancellationToken);
            if (userProfile is null)
            {
                _logger.LogWarning("[NotificationAPI] não foi encontrado para {UserId} ao processar pagamento rejeitado do pedido {OrderId}", command.UserId, command.OrderId);
                throw new IntegrationEventProcessingException($"Usuário não encontrado {command.UserId}.");
            }

            var emailRecipient = new EmailAddress(userProfile.Email);


            var notification = new NotificationMessage(userProfile.UserId, emailRecipient, NotificationType.OrderConfirmation);

            var (subject, body) = notification.GeneratePaymentRejectionContent(command.OrderId, userProfile.Name, command.Reason);

            await _emailService.SendEmailAsync(notification.Recipient, subject, body, cancellationToken);

        }
    }
}
