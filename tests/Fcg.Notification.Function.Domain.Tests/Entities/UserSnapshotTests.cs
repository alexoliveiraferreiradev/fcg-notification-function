using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Notification.Function.Domain.Entities;
using FluentAssertions;

namespace Fcg.Notification.Function.Domain.Tests.Entities
{
    public class UserSnapshotTests
    {
        public string GetName() => "Nome Usuario";
        public string GetEmail() => "teste@email.com";
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            //Act
            var userSnapshot = new UserSnapshot(GetName(), GetEmail());
            //Arrange
            Assert.NotEmpty(userSnapshot.Name);
            Assert.NotEmpty(userSnapshot.Email);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenNameIsNullOrEmpty()
        {
            // Arrange
            string name = null;
            string email = GetEmail();
            // Act & Assert
            Assert.Throws<DomainException>(() => new UserSnapshot(name, email));
        }

        [Fact]
        public void ApplyChanges_ShouldUpdateProperties()
        {
            // Arrange
            var userSnapshot = new UserSnapshot(GetName(), GetEmail());
            var newName = "Novo Nome";
            var ocurredAt = DateTime.UtcNow;
            // Act
            userSnapshot.ApplyChanges(newName, ocurredAt);
            // Assert
            Assert.Equal(newName, userSnapshot.Name);
            Assert.Equal(ocurredAt, userSnapshot.LastSyncedAt);
        }

        [Fact]
        public void ApplyChanges_ShouldThrowDomainException_WhenNameIsNullOrEmpty()
        {
            // Arrange
            var userSnapshot = new UserSnapshot(GetName(), GetEmail());
            string newName = null;
            var ocurredAt = DateTime.UtcNow;
            // Act & Assert
            Assert.Throws<DomainException>(() => userSnapshot.ApplyChanges(newName, ocurredAt));
        }

        [Fact]
        public void ApplyChanges_ShouldNotUpdateProperties_WhenOcurredIsEarlierThanLastSyncedAt()
        {
            // Arrange
            var userSnapshot = new UserSnapshot(GetName(), GetEmail());
            var recentDate = DateTime.UtcNow;
            var newName = "Novo Nome";
            userSnapshot.ApplyChanges(newName, recentDate);
            var earlierDate = recentDate.AddHours(-1);
            // Act 
            userSnapshot.ApplyChanges("Outro Nome", earlierDate);
            //Assert
            userSnapshot.Name.Should().Be("Novo Nome");
            userSnapshot.LastSyncedAt.Should().Be(recentDate);
        }

        [Fact]
        public void ApplyChanges_ShouldNotUpdateProperties_WhenOcurredIsEarlierThanLastSyncedAtAndNameIsNull()
        {
            // Arrange
            var userSnapshot = new UserSnapshot(GetName(), GetEmail());
            var recentDate = DateTime.UtcNow;
            var newName = "Novo Nome";
            userSnapshot.ApplyChanges(newName, recentDate);
            var laterDate = recentDate.AddHours(1);
            // Act 
            var act = () => userSnapshot.ApplyChanges(null!, laterDate);
            //Assert
            Assert.ThrowsAny<DomainException>(act);
            userSnapshot.Name.Should().Be("Novo Nome");
        }

        [Fact]
        public void ApplyChanges_ShouldUpdateProperties_WhenOcurredDateAndLastSyncedAtAreEqual()
        {
            // Arrange
            var userSnapshot = new UserSnapshot(GetName(), GetEmail());
            var recentDate = DateTime.UtcNow;
            var newName = "Novo Nome";
            userSnapshot.ApplyChanges(newName, recentDate);
            // Act 
            userSnapshot.ApplyChanges("Outro Nome", recentDate);
            //Assert
            userSnapshot.Name.Should().Be("Outro Nome");
            userSnapshot.LastSyncedAt.Should().Be(recentDate);
        }
    }
}
