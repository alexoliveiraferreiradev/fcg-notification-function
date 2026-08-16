using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Resources;
using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Domain.Entities;
using Fcg.Notification.Function.Domain.Enum;
using Fcg.Notification.Function.Domain.Repositories;
using Fcg.Notification.Function.Domain.ValueObject;
using MediatR;

namespace Fcg.Notification.Function.Application.UseCase.DeliveryFailedEmail
{
    public class SendDeliveryFailedEmailCommandHandler : IRequestHandler<SendDeliveryFailedEmailCommand>
    {
        private readonly IEmailService _emailService;
        private readonly IUserSnapshotRepository _userSnapshotRepository;

        public SendDeliveryFailedEmailCommandHandler(IEmailService emailService,
            IUserSnapshotRepository userSnapshotRepository)
        {
            _emailService = emailService;
            _userSnapshotRepository = userSnapshotRepository;
        }

        public async Task Handle(SendDeliveryFailedEmailCommand command, CancellationToken cancellationToken)
        {
            var userProfile = await _userSnapshotRepository.GetByUserIdAsync(command.UserId, cancellationToken);
            if (userProfile == null) throw new DomainException(DomainMessages.UserNotFound);

            var emailRecipient = new EmailAddress(userProfile.Email);

            var notification = new NotificationMessage(userProfile.UserId,emailRecipient, NotificationType.OrderConfirmation);

            var (subject, body) = notification.GenerateDeliveryFailedContent(command.OrderId, userProfile.Name);

            await _emailService.SendEmailAsync(notification.Recipient, subject, body, cancellationToken);


        }
    }
}
