using Fcg.Notification.Function.Application.Common.Interfaces;
using MediatR;

namespace Fcg.Notification.Function.Application.UseCase.ApprovedPaymentEmail
{
    public record SendPaymentApprovedEmailCommand(Guid EventId, Guid UsuarioId, Guid OrderId, DateTime CreatedAt) : IRequest,IIdempotentCommand;
}
