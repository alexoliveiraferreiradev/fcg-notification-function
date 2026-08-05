using Fcg.Core.Abstractions.Common;
using Fcg.Core.Abstractions.Resources;

namespace Fcg.Notification.Function.Domain.ValueObject
{
    public class EmailAddress : ValueObject<EmailAddress>
    {
        public string Address { get; }

        public EmailAddress(string address)
        {
            AssertionConcern.AssertArgumentEmailFormat(address, DomainMessages.EmailInvalid);
            Address = address;
        }
        protected override bool EqualsCore(EmailAddress other)
        {
            return Address == other.Address;
        }

        protected override int GetHashCodeCore()
        {
           return Address.GetHashCode();    
        }
    }
}
