using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Interfaces;
using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Application.Ports;
using Fcg.Notification.Function.Domain.Entities;
using Fcg.Notification.Function.Domain.Enum;
using Fcg.Notification.Function.Domain.Repositories;
using Fcg.Notification.Function.Domain.ValueObject;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Notification.Function.Application.UseCase.ApprovedPaymentEmail
{
    public class SendPaymentApprovedEmailCommandHandler : IRequestHandler<SendPaymentApprovedEmailCommand>
    {
        private readonly IEmailService _emailService;
        private readonly IUserSnapshotRepository _userSnapshotRepository;
        private readonly ILogger<SendPaymentApprovedEmailCommandHandler> _logger;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SendPaymentApprovedEmailCommandHandler(IEmailService emailService,
             IUserSnapshotRepository userSnapshotRepository, ILogger<SendPaymentApprovedEmailCommandHandler> logger, INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
        {
            _emailService = emailService;
            _userSnapshotRepository = userSnapshotRepository;
            _logger = logger;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(SendPaymentApprovedEmailCommand command, CancellationToken cancellationToken)
        {
            var userProfile = await _userSnapshotRepository.GetByUserIdAsync(command.UsuarioId, cancellationToken);

            if (userProfile is null)
            {
                _logger.LogWarning("[NotificationAPI] não foi encontrado para {UserId} ao processar pagamento aprovado do pedido {OrderId}", command.UsuarioId, command.OrderId);
                throw new IntegrationEventProcessingException($"Usuário não encontrado {command.UsuarioId}.");
            }

            var emailRecipient = new EmailAddress(userProfile.Email);

            var notification = new NotificationMessage(emailRecipient, NotificationType.OrderConfirmation);

            _notificationRepository.Add(notification);

            var (subject, body) = notification.GenerateOrderConfirmationContent(command.OrderId, userProfile.Name, command.CreatedAt);

            await _emailService.SendEmailAsync(notification.Recipient, subject, body, cancellationToken);

            notification.MarkAsSent();

            await _unitOfWork.CommitAsync();

        }
    }
}
