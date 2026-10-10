using Fcg.Core.Abstractions.Interfaces;
using Fcg.Notification.Function.Application.UseCase.WelcomeEmail;
using Fcg.Notification.Function.Domain.Entities;
using Fcg.Notification.Function.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Fcg.Notification.Function.Application.Tests.UseCase.WelcomeEmail
{
    public class RegisterUserHandlerTests
    {
        private readonly Mock<IUserSnapshotRepository> _userSnapshotRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly RegisterUserCommandHandler _useCase;

        public RegisterUserHandlerTests()
        {
            _userSnapshotRepositoryMock = new Mock<IUserSnapshotRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _useCase = new RegisterUserCommandHandler(_userSnapshotRepositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldAddSnapshotWithLastSyncedAt_WhenUserDoesNotExist()
        {
            // Arrange
            var command = new RegisterUserCommand(Guid.NewGuid(), Guid.NewGuid(), "Usuario Teste", "teste@email.com", DateTime.UtcNow);
            UserSnapshot? added = null;
            _userSnapshotRepositoryMock.Setup(r => r.GetByUserIdAsync(command.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserSnapshot?)null);
            _userSnapshotRepositoryMock.Setup(r => r.Add(It.IsAny<UserSnapshot>()))
                .Callback<UserSnapshot>(s => added = s);

            // Act
            await _useCase.Handle(command, CancellationToken.None);

            // Assert
            added.Should().NotBeNull();
            added!.UserId.Should().Be(command.UserId);
            added.LastSyncedAt.Should().Be(command.occurredAt);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldUpdateNameAndEmail_WhenUserAlreadyExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var snapshot = new UserSnapshot(userId, "Nome Antigo", "antigo@email.com", DateTime.UtcNow.AddDays(-1));
            var command = new RegisterUserCommand(Guid.NewGuid(), userId, "Nome Novo", "novo@email.com", DateTime.UtcNow);
            _userSnapshotRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(snapshot);

            // Act
            await _useCase.Handle(command, CancellationToken.None);

            // Assert
            snapshot.Name.Should().Be("Nome Novo");
            snapshot.Email.Should().Be("novo@email.com");
            snapshot.LastSyncedAt.Should().Be(command.occurredAt);
            _userSnapshotRepositoryMock.Verify(r => r.Add(It.IsAny<UserSnapshot>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}
