using Fcg.Core.Abstractions.Interfaces;
using Fcg.Notification.Function.Application.Behaviors;
using Fcg.Notification.Function.Application.Common.Exceptions;
using Fcg.Notification.Function.Application.Ports;
using Fcg.Notification.Function.Application.UseCase.ApprovedPaymentEmail;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fcg.Notification.Function.Application.Tests.Behaviors
{
    public class IdempotencyBehaviorTests
    {
        private readonly CancellationToken _cancellationToken = CancellationToken.None;
        private readonly Mock<IIdempotencyService> _idempotencyServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IdempotencyBehavior<SendPaymentApprovedEmailCommand, Unit> _behavior;
        private readonly SendPaymentApprovedEmailCommand _command;
        private readonly string _idempotencyKey;

        public IdempotencyBehaviorTests()
        {
            _idempotencyServiceMock = new Mock<IIdempotencyService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _behavior = new IdempotencyBehavior<SendPaymentApprovedEmailCommand, Unit>(
                _idempotencyServiceMock.Object,
                _unitOfWorkMock.Object,
                Mock.Of<ILogger<IdempotencyBehavior<SendPaymentApprovedEmailCommand, Unit>>>());

            _command = new SendPaymentApprovedEmailCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
            _idempotencyKey = $"{nameof(SendPaymentApprovedEmailCommand)}:{_command.EventId}";
        }

        private void SetupCanProcess(bool canProcess) =>
            _idempotencyServiceMock.Setup(s => s.TryProcessAsync(_idempotencyKey, _cancellationToken)).ReturnsAsync(canProcess);

        [Fact]
        public async Task Handle_ShouldNotCallNext_WhenAlreadyProcessed()
        {
            //Arrange
            SetupCanProcess(false);
            var nextCalled = false;
            RequestHandlerDelegate<Unit> next = _ =>
            {
                nextCalled = true;
                return Task.FromResult(Unit.Value);
            };

            //Act
            var result = await _behavior.Handle(_command, next, _cancellationToken);

            //Assert
            result.Should().Be(Unit.Value);
            nextCalled.Should().BeFalse();
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldCallNextAndCommit_WhenNotProcessed()
        {
            //Arrange
            SetupCanProcess(true);
            var nextCalled = false;
            RequestHandlerDelegate<Unit> next = _ =>
            {
                nextCalled = true;
                return Task.FromResult(Unit.Value);
            };

            //Act
            await _behavior.Handle(_command, next, _cancellationToken);

            //Assert
            nextCalled.Should().BeTrue();
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotCommit_WhenNextThrowsException()
        {
            //Arrange
            SetupCanProcess(true);
            RequestHandlerDelegate<Unit> next = _ => throw new InvalidOperationException("Test exception");

            //Act
            var act = () => _behavior.Handle(_command, next, _cancellationToken);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldIgnore_WhenCommitDetectsDuplicate()
        {
            //Arrange
            SetupCanProcess(true);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ThrowsAsync(new DuplicateMessageException());
            RequestHandlerDelegate<Unit> next = _ => Task.FromResult(Unit.Value);

            //Act
            var act = () => _behavior.Handle(_command, next, _cancellationToken);

            //Assert
            (await act.Should().NotThrowAsync()).Which.Should().Be(Unit.Value);
        }

        [Fact]
        public async Task Handle_ShouldIgnore_WhenHandlerCommitDetectsDuplicate()
        {
            //Arrange
            SetupCanProcess(true);
            RequestHandlerDelegate<Unit> next = _ => throw new DuplicateMessageException();

            //Act
            var act = () => _behavior.Handle(_command, next, _cancellationToken);

            //Assert
            await act.Should().NotThrowAsync();
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}
