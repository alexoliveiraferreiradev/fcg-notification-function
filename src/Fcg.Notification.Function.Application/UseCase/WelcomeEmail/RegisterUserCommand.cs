using Fcg.Notification.Function.Application.Common.Interfaces;
using MediatR;

namespace Fcg.Notification.Function.Application.UseCase.WelcomeEmail
{
    public record RegisterUserCommand(Guid EventId, Guid UserId, string UserName, string Email, DateTime occurredAt) : IRequest, IIdempotentCommand;
}
