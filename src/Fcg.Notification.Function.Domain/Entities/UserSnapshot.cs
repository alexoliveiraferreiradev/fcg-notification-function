using Fcg.Core.Abstractions.Common;
using Fcg.Core.Abstractions.Resources;

namespace Fcg.Notification.Function.Domain.Entities
{
    public class UserSnapshot : AggregateRoot
    {
        public Guid UserId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public DateTime LastSyncedAt { get; private set; }

        protected UserSnapshot() { }

        public UserSnapshot(Guid userId, string name, string email, DateTime occurredAt)
        {
            UserId = userId;
            Name = name;
            Email = email;
            LastSyncedAt = occurredAt;
            ValidateEntity();
        }

        public void ApplyChanges(string newName, string newEmail, DateTime occurredAt)
        {
            if (occurredAt < LastSyncedAt) return;
            AssertionConcern.AssertArgumentNotNull(newName, DomainMessages.UserNameRequired);
            AssertionConcern.AssertArgumentNotNull(newEmail, DomainMessages.UserEmailRequired);
            Name = newName;
            Email = newEmail;
            LastSyncedAt = occurredAt;
        }

        protected override void ValidateEntity()
        {
            AssertionConcern.AssertArgumentNotNull(Name, DomainMessages.UserNameRequired);
            AssertionConcern.AssertArgumentNotNull(Email, DomainMessages.UserEmailRequired);
        }
    }
}
