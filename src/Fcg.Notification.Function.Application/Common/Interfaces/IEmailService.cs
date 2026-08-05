using Fcg.Notification.Function.Domain.ValueObject;

namespace Fcg.Notification.Function.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailAddress recipient, string subject, string body, CancellationToken cancellationToken);       
    }
}
