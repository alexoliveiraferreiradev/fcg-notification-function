using Fcg.Notification.Function.Application.Behaviors;
using Fcg.Notification.Function.Application.Ports;
using Fcg.Notification.Function.Application.UseCase.ApprovedPaymentEmail;
using Fcg.Notification.Function.Application.UseCase.WelcomeEmail;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fcg.Notification.Function.Application.Tests.Behaviors
{
    public class IdempotencyBehaviorTests
    {
        private readonly Mock<IIdempotencyService> _idempotencyServiceMock;
        private readonly IdempotencyBehavior<SendPaymentApprovedEmailCommand, Unit> _behavior;
        public IdempotencyBehaviorTests()
        {
            _idempotencyServiceMock = new Mock<IIdempotencyService>();
            _behavior = new IdempotencyBehavior<SendPaymentApprovedEmailCommand, Unit>(
                _idempotencyServiceMock.Object, 
                Mock.Of<ILogger<IdempotencyBehavior<SendPaymentApprovedEmailCommand, Unit>>>());
        }

        public SendPaymentApprovedEmailCommand PaymentCommand  ()=> new SendPaymentApprovedEmailCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        [Fact]
        public async Task Handle_ShouldNotCallNext_WhenAlreadyProcessed()
        {
            //Arrange
            var command = PaymentCommand();
            var idempotencyKey = $"{typeof(SendPaymentApprovedEmailCommand).Name}:{command.EventId}";
            _idempotencyServiceMock.Setup(s => s.TryProcessAsync(idempotencyKey)).ReturnsAsync(false);

            var nextCalled = false;
            RequestHandlerDelegate<Unit> next = _ =>
            {
                nextCalled = true;
                return Task.FromResult(Unit.Value);
            };

            //Act            
            await _behavior.Handle(command, next, CancellationToken.None);

            //Assert
            nextCalled.Should().BeFalse();
            _idempotencyServiceMock.Verify(s => s.TryProcessAsync(idempotencyKey), Times.Once);
            _idempotencyServiceMock.Verify(s => s.ReleaseAsync(idempotencyKey), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReleaseLock_WhenNextThrowException()
        {
            //Arrange             
            var command = PaymentCommand();
            var idempotencyKey = $"{typeof(SendPaymentApprovedEmailCommand).Name}:{command.EventId}";
            _idempotencyServiceMock.Setup(s => s.TryProcessAsync(idempotencyKey)).ReturnsAsync(true);

            RequestHandlerDelegate<Unit> next = _ => throw new InvalidOperationException("Test exception");

            //Act
            var act = () => _behavior.Handle(command, next,CancellationToken.None);

            //Assert    
            await act.Should().ThrowAsync<InvalidOperationException>();
            _idempotencyServiceMock.Verify(s => s.ReleaseAsync(idempotencyKey), Times.Once);
        }
    }

}

