using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Notification.Function.Domain.Entities;
using FluentAssertions;

namespace Fcg.Notification.Function.Domain.Tests.Entities
{
    public class UserSnapshotTests
    {
        public string GetName() => "Nome Usuario";
        public string GetEmail() => "teste@email.com";
        public DateTime GetCreationDate() => DateTime.UtcNow.AddDays(-1);

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            var occurredAt = GetCreationDate();
            //Act
            var userSnapshot = new UserSnapshot(Guid.NewGuid(), GetName(), GetEmail(), occurredAt);
            //Assert
            Assert.NotEmpty(userSnapshot.Name);
            Assert.NotEmpty(userSnapshot.Email);
            Assert.Equal(occurredAt, userSnapshot.LastSyncedAt);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenNameIsNullOrEmpty()
        {
            // Arrange
            string name = null;
            string email = GetEmail();
            // Act & Assert
            Assert.Throws<DomainException>(() => new UserSnapshot(Guid.NewGuid(), name, email, GetCreationDate()));
        }

        [Fact]
        public void ApplyChanges_ShouldUpdateProperties()
        {
            // Arrange
            var userSnapshot = new UserSnapshot(Guid.NewGuid(), GetName(), GetEmail(), GetCreationDate());
            var newName = "Novo Nome";
            var newEmail = "novo@email.com";
            var ocurredAt = DateTime.UtcNow;
            // Act
            userSnapshot.ApplyChanges(newName, newEmail, ocurredAt);
            // Assert
            Assert.Equal(newName, userSnapshot.Name);
            Assert.Equal(newEmail, userSnapshot.Email);
            Assert.Equal(ocurredAt, userSnapshot.LastSyncedAt);
        }

        [Fact]
        public void ApplyChanges_ShouldThrowDomainException_WhenNameIsNullOrEmpty()
        {
            // Arrange
            var userSnapshot = new UserSnapshot(Guid.NewGuid(), GetName(), GetEmail(), GetCreationDate());
            string newName = null;
            var ocurredAt = DateTime.UtcNow;
            // Act & Assert
            Assert.Throws<DomainException>(() => userSnapshot.ApplyChanges(newName, GetEmail(), ocurredAt));
        }

        [Fact]
        public void ApplyChanges_ShouldThrowDomainException_WhenEmailIsNullOrEmpty()
        {
            // Arrange
            var userSnapshot = new UserSnapshot(Guid.NewGuid(), GetName(), GetEmail(), GetCreationDate());
            var ocurredAt = DateTime.UtcNow;
            // Act & Assert
            Assert.Throws<DomainException>(() => userSnapshot.ApplyChanges(GetName(), null!, ocurredAt));
            userSnapshot.Email.Should().Be(GetEmail());
        }

        [Fact]
        public void ApplyChanges_ShouldNotUpdateProperties_WhenOcurredIsEarlierThanCreation()
        {
            // Arrange
            var creationDate = GetCreationDate();
            var userSnapshot = new UserSnapshot(Guid.NewGuid(), GetName(), GetEmail(), creationDate);
            // Act
            userSnapshot.ApplyChanges("Outro Nome", "outro@email.com", creationDate.AddHours(-1));
            // Assert
            userSnapshot.Name.Should().Be(GetName());
            userSnapshot.Email.Should().Be(GetEmail());
            userSnapshot.LastSyncedAt.Should().Be(creationDate);
        }

        [Fact]
        public void ApplyChanges_ShouldNotUpdateProperties_WhenOcurredIsEarlierThanLastSyncedAt()
        {
            // Arrange
            var userSnapshot = new UserSnapshot(Guid.NewGuid(), GetName(), GetEmail(), GetCreationDate());
            var recentDate = DateTime.UtcNow;
            var newName = "Novo Nome";
            userSnapshot.ApplyChanges(newName, GetEmail(), recentDate);
            var earlierDate = recentDate.AddHours(-1);
            // Act
            userSnapshot.ApplyChanges("Outro Nome", GetEmail(), earlierDate);
            //Assert
            userSnapshot.Name.Should().Be("Novo Nome");
            userSnapshot.LastSyncedAt.Should().Be(recentDate);
        }

        [Fact]
        public void ApplyChanges_ShouldNotUpdateProperties_WhenOcurredIsEarlierThanLastSyncedAtAndNameIsNull()
        {
            // Arrange
            var userSnapshot = new UserSnapshot(Guid.NewGuid(), GetName(), GetEmail(), GetCreationDate());
            var recentDate = DateTime.UtcNow;
            var newName = "Novo Nome";
            userSnapshot.ApplyChanges(newName, GetEmail(), recentDate);
            var laterDate = recentDate.AddHours(1);
            // Act
            var act = () => userSnapshot.ApplyChanges(null!, GetEmail(), laterDate);
            //Assert
            Assert.ThrowsAny<DomainException>(act);
            userSnapshot.Name.Should().Be("Novo Nome");
        }

        [Fact]
        public void ApplyChanges_ShouldUpdateProperties_WhenOcurredDateAndLastSyncedAtAreEqual()
        {
            // Arrange
            var userSnapshot = new UserSnapshot(Guid.NewGuid(), GetName(), GetEmail(), GetCreationDate());
            var recentDate = DateTime.UtcNow;
            var newName = "Novo Nome";
            userSnapshot.ApplyChanges(newName, GetEmail(), recentDate);
            // Act
            userSnapshot.ApplyChanges("Outro Nome", GetEmail(), recentDate);
            //Assert
            userSnapshot.Name.Should().Be("Outro Nome");
            userSnapshot.LastSyncedAt.Should().Be(recentDate);
        }
    }
}
