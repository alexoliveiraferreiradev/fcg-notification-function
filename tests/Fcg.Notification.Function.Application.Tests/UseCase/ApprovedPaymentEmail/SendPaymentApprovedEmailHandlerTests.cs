using Fcg.Core.Abstractions.Interfaces;
using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Application.UseCase.ApprovedPaymentEmail;
using Fcg.Notification.Function.Domain.Entities;
using Fcg.Notification.Function.Domain.Enum;
using Fcg.Notification.Function.Domain.Repositories;
using Fcg.Notification.Function.Domain.ValueObject;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fcg.Notification.Function.Application.Tests.UseCase.ApprovedPaymentEmail
{
    public class SendPaymentApprovedEmailHandlerTests
    {
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IUserSnapshotRepository> _userSnaphotRepositoryMock;
        private readonly SendPaymentApprovedEmailCommandHandler _useCase;
        private readonly Mock<ILogger<SendPaymentApprovedEmailCommandHandler>> _logger;
        private readonly Mock<INotificationRepository> _notificationRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private EmailAddress ObtemEmailValido() => new EmailAddress("alex@email.com");
        public SendPaymentApprovedEmailHandlerTests()
        {
            _emailServiceMock = new Mock<IEmailService>();
            _userSnaphotRepositoryMock = new Mock<IUserSnapshotRepository>();
            _logger = new Mock<ILogger<SendPaymentApprovedEmailCommandHandler>>();
            _notificationRepositoryMock = new Mock<INotificationRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();  

            _useCase = new SendPaymentApprovedEmailCommandHandler(
                _emailServiceMock.Object,
                _userSnaphotRepositoryMock.Object,
                _logger.Object,
                _notificationRepositoryMock.Object,
                _unitOfWorkMock.Object
            );
        }


        [Fact]
        public async Task Handle_ShouldSendEmailAndConfirmTransaction_WhenIsValid()
        {
            // Arrange
            var command = new SendPaymentApprovedEmailCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),  DateTime.UtcNow);

            var emailValido = ObtemEmailValido().Address;   

            var userSnapshot = new UserSnapshot(Guid.NewGuid(),"Usuario Teste", emailValido);

            _userSnaphotRepositoryMock.Setup(r=>r.GetByUserIdAsync(command.UsuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userSnapshot);

            // Act
            await _useCase.Handle(command, CancellationToken.None);

            // Assert
            _userSnaphotRepositoryMock.Verify(
                r => r.GetByUserIdAsync(command.UsuarioId, It.IsAny<CancellationToken>()), Times.Once);

            _notificationRepositoryMock.Verify(
                r => r.Add(It.Is<NotificationMessage>(n=>
                     n.Recipient.Address == userSnapshot.Email && n.Status == NotificationStatus.Sent)),
                Times.Once);

            _emailServiceMock.Verify(e => e.SendEmailAsync(
                It.Is<EmailAddress>(addr=> addr.Address == userSnapshot.Email), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldSendEmailAndMarkAsProcessed_WhenHappyPath()
        {
            // Arrange
            var command = new SendPaymentApprovedEmailCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
          
            var emailValido = ObtemEmailValido().Address;
            
            var userSnapshot = new UserSnapshot(Guid.NewGuid(),"Usuario Teste", emailValido);
            _userSnaphotRepositoryMock
                .Setup(s => s.GetByUserIdAsync(command.UsuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userSnapshot);

            // Act
            await _useCase.Handle(command, CancellationToken.None);

            // Assert
            _emailServiceMock.Verify(e => e.SendEmailAsync(
                It.Is<EmailAddress>(addr => addr.Address == emailValido), 
                It.Is<string>(s => s.Contains(command.OrderId.ToString())), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()), Times.Once);


            _userSnaphotRepositoryMock.Verify(s => s.GetByUserIdAsync(command.UsuarioId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenDeliveryFailed()
        {
            // Arrange
            var command = new SendPaymentApprovedEmailCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),  DateTime.UtcNow);
            
            var emailValido = ObtemEmailValido().Address;

            var userSnapshot = new UserSnapshot(Guid.NewGuid(),"Usuario Teste", emailValido);
            _userSnaphotRepositoryMock
                .Setup(s => s.GetByUserIdAsync(command.UsuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userSnapshot);

            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<EmailAddress>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("SMTP Timeout"));

            // Act
            Func<Task> act = async () => await _useCase.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("SMTP Timeout");
            _userSnaphotRepositoryMock.Verify(s => s.GetByUserIdAsync(command.UsuarioId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
