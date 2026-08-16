using Fcg.Core.Abstractions.Common;
using Fcg.Core.Abstractions.Resources;
using Fcg.Notification.Function.Domain.ValueObject;

namespace Fcg.Notification.Function.Domain.Entities
{
    public class UserSnapshot : AggregateRoot
    {
        public Guid UserId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public DateTime LastSyncedAt { get; private set; }

        public UserSnapshot(Guid userId, string name, string email)
        {
            UserId = userId;
            Name = name;
            Email = email;
            ValidateEntity();
        }

        public void ApplyChanges(string newName, DateTime occurredAt)
        {
            if (occurredAt < LastSyncedAt) return;
            AssertionConcern.AssertArgumentNotNull(newName, DomainMessages.UserNameRequired);
            Name = newName;
            LastSyncedAt = occurredAt;
        }

        protected override void ValidateEntity()
        {
            AssertionConcern.AssertArgumentNotNull(Name, DomainMessages.UserNameRequired);
            AssertionConcern.AssertArgumentNotNull(Email, DomainMessages.UserEmailRequired);    
        }
    }
}
