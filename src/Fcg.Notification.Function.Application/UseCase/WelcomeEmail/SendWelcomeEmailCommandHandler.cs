using Fcg.Core.Abstractions.Interfaces;
using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Application.Ports;
using Fcg.Notification.Function.Domain.Entities;
using Fcg.Notification.Function.Domain.Enum;
using Fcg.Notification.Function.Domain.Repositories;
using Fcg.Notification.Function.Domain.ValueObject;
using MediatR;

namespace Fcg.Notification.Function.Application.UseCase.WelcomeEmail
{
    public class SendWelcomeEmailCommandHandler : IRequestHandler<SendWelcomeEmailCommand,Unit>
    {
        private readonly IEmailService _emailService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        public SendWelcomeEmailCommandHandler(IEmailService emailService,
            INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
        {
            _emailService = emailService;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(SendWelcomeEmailCommand command, CancellationToken cancellationToken)
        {
            var emailRecipient = new EmailAddress(command.Email);

            var notification = new NotificationMessage(emailRecipient, NotificationType.Welcome);

            _notificationRepository.Add(notification);

            var (subject, body) = notification.GenerateWelcomeContent(command.UserName);
            await _emailService.SendEmailAsync(notification.Recipient, subject, body, cancellationToken);

            notification.MarkAsSent();

            await _unitOfWork.CommitAsync();
            return Unit.Value;
        }
    }
}

