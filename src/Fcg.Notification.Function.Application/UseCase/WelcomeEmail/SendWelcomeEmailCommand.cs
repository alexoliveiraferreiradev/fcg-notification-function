using Fcg.Notification.Function.Application.Common.Interfaces;
using MediatR;

namespace Fcg.Notification.Function.Application.UseCase.WelcomeEmail
{
    public record SendWelcomeEmailCommand(Guid EventId, Guid UserId, string Email, string UserName) : IRequest<Unit>, IIdempotentCommand;
}
