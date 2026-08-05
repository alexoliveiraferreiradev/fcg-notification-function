using Fcg.Notification.Function.Application.Common.Interfaces;
using MediatR;

namespace Fcg.Notification.Function.Application.UseCase.PaymentRejectEmail
{
    public record SendPaymentRejectCommand(Guid EventId, Guid OrderId, Guid UserId, string Reason) : IRequest,IIdempotentCommand;
}
