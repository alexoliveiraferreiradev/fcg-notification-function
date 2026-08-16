using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Interfaces;
using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Domain.Entities;
using Fcg.Notification.Function.Domain.Enum;
using Fcg.Notification.Function.Domain.Repositories;
using Fcg.Notification.Function.Domain.ValueObject;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Notification.Function.Application.UseCase.WelcomeEmail
{
    public class SendWelcomeEmailCommandHandler : IRequestHandler<SendWelcomeEmailCommand,Unit>
    {
        private readonly IEmailService _emailService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SendWelcomeEmailCommandHandler> _logger;
        private readonly IUserSnapshotRepository _userSnapshotRepository;
        public SendWelcomeEmailCommandHandler(IEmailService emailService,
            INotificationRepository notificationRepository, IUnitOfWork unitOfWork,
            ILogger<SendWelcomeEmailCommandHandler> logger, IUserSnapshotRepository userSnapshotRepository)
        {
            _emailService = emailService;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userSnapshotRepository = userSnapshotRepository;
        }

        public async Task<Unit> Handle(SendWelcomeEmailCommand command, CancellationToken cancellationToken)
        {
            var userProfile = await _userSnapshotRepository.GetByUserIdAsync(command.UserId, cancellationToken);
            if (userProfile is null)
            {
                _logger.LogWarning("[NotificationAPI] não foi encontrado para {UserId} ao enviar mensagem de boas vindas", command.UserId);
                throw new IntegrationEventProcessingException($"Usuário não encontrado {command.UserId}.");
            }

            var emailRecipient = new EmailAddress(command.Email);

            var notification = new NotificationMessage(userProfile.UserId, emailRecipient, NotificationType.Welcome);

            _notificationRepository.Add(notification);

            var (subject, body) = notification.GenerateWelcomeContent(command.UserName);
            await _emailService.SendEmailAsync(notification.Recipient, subject, body, cancellationToken);

            notification.MarkAsSent();

            await _unitOfWork.CommitAsync();
            return Unit.Value;
        }
    }
}

