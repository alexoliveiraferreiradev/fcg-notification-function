using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Notification.Function.Application.UseCase.PaymentRejectEmail;
using MassTransit;
using MediatR;

namespace Fcg.Notification.Function.Infrastructure.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly ISender _sender;

        public PaymentFailedEventConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var mensagem = context.Message;

            var command = new SendPaymentRejectCommand(
                EventId: mensagem.EventId, 
                OrderId: mensagem.OrderId,
                UserId: mensagem.UserId, 
                Reason: mensagem.Reason);

            await _sender.Send(command, context.CancellationToken);
        }
    }
}
