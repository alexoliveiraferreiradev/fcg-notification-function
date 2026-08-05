using Fcg.Notification.Function.Application.Common.Interfaces;
using MediatR;

namespace Fcg.Notification.Function.Application.UseCase.DeliveryFailedEmail
{
    public record SendDeliveryFailedEmailCommand(Guid EventId, Guid OrderId, Guid UserId, string Reason) : IRequest,IIdempotentCommand;
}
