using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Application.UseCase.ApprovedPaymentEmail;
using MassTransit;
using MediatR;

namespace Fcg.Notification.Function.Infrastructure.Consumers
{
    public class PaymentProcessedEventConsumer : IConsumer<PaymentProcessedEvent>
    {
        private readonly ISender _sender;

        public PaymentProcessedEventConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
        {
            var mensagem = context.Message;

            var command = new SendPaymentApprovedEmailCommand(
                EventId: mensagem.EventId,
                UsuarioId: mensagem.UserId,
                OrderId: mensagem.OrderId,
                CreatedAt: mensagem.CreatedAt);

            await _sender.Send(command, context.CancellationToken);
        }
    }
}
