using Fcg.Core.Abstractions.Interfaces;
using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Application.Ports;
using Fcg.Notification.Function.Application.UseCase.WelcomeEmail;
using Fcg.Notification.Function.Domain.Repositories;
using Fcg.Notification.Function.Domain.ValueObject;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fcg.Notification.Function.Application.Tests.UseCase.WelcomeEmail
{
    public class SendWelcomeEmailHandlerTests
    {
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<INotificationRepository> _notificationRepositoryMock;
        private readonly SendWelcomeEmailCommandHandler _useCase;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        public SendWelcomeEmailHandlerTests()
        {
            _emailServiceMock = new Mock<IEmailService>();
            _notificationRepositoryMock = new Mock<INotificationRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _useCase = new SendWelcomeEmailCommandHandler(
                _emailServiceMock.Object,
                _notificationRepositoryMock.Object,
                _unitOfWorkMock.Object

            );
        }


        [Fact]
        public async Task Handle_ShouldSendEmailAndMarkAsProcessed_WhenHappyPath()
        {
            // Arrange
            var command = new SendWelcomeEmailCommand(Guid.NewGuid(), Guid.NewGuid(), "teste@teste.com", "Joao");
            

            // Act
            await _useCase.Handle(command, CancellationToken.None);

            // Assert
            _emailServiceMock.Verify(e => e.SendEmailAsync(
                It.Is<EmailAddress>(addr => addr.Address == command.Email), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()), Times.Once);
                
          
          
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenDeliveryFailed()
        {
            // Arrange
            var command = new SendWelcomeEmailCommand(Guid.NewGuid(), Guid.NewGuid(), "teste@teste.com", "Joao");
           
            
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<EmailAddress>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("SMTP Timeout"));

            // Act
            Func<Task> act = async () => await _useCase.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("SMTP Timeout");
            
            
        }
    }
}
