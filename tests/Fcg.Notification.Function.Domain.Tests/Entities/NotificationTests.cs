using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Resources;
using Fcg.Notification.Function.Domain.Entities;
using Fcg.Notification.Function.Domain.Enum;
using Fcg.Notification.Function.Domain.ValueObject;
using FluentAssertions;

namespace Fcg.Notification.Function.Domain.Tests.Entities
{
    public class NotificationTests
    {
        public EmailAddress GetEmail() => new EmailAddress("test@fiap.com");
        [Fact]
        public void Construtor_ShouldInitializeWithPendingStatus_And_NoSendDate()
        {
            // Arrange
            // Act
            var notification = new NotificationMessage(GetEmail(), NotificationType.Welcome);

            // Assert
            notification.Id.Should().NotBeEmpty();
            notification.Recipient.Address.Should().Be("test@fiap.com");
            notification.Type.Should().Be(NotificationType.Welcome);
            notification.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            notification.SentAt.Should().BeNull();
        }

        [Fact]
        public void Construtor_ShouldThrowDomainException_WhenEmailIsInvalid()
        {
            // Arrange
            Action act = () => new NotificationMessage(new EmailAddress("invalid-email"), NotificationType.Welcome);
            // Act & Assert
            act.Should().Throw<DomainException>().WithMessage(DomainMessages.EmailInvalid);
        }

        [Fact]
        public void MarkAsSent_ShouldUpdateStatusAndSentAt()
        {
            // Arrange
            var notification = new NotificationMessage(GetEmail(), NotificationType.Welcome);
            // Act
            notification.MarkAsSent();
            // Assert
            notification.SentAt.Should().NotBeNull();
        }

        [Fact]
        public void MarkAsSent_ShouldThrowDomainException_WhenAlreadySent()
        {
            // Arrange
            var notification = new NotificationMessage(GetEmail(), NotificationType.Welcome);
            notification.MarkAsSent();
            // Act
            Action act = () => notification.MarkAsSent();
            // Assert
            act.Should().Throw<DomainException>().WithMessage("Notificação já foi marcada com enviada.");
        }
    }
}

