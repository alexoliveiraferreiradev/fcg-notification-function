using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Application.UseCase.DeliveryFailedEmail;
using MassTransit;
using MediatR;

namespace Fcg.Notification.Function.Infrastructure.Consumers
{
    public class DeliveryFailedEventConsumer : IConsumer<DeliveryFailedEvent>
    {
        private readonly ISender _sender;

        public DeliveryFailedEventConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<DeliveryFailedEvent> context)
        {
            var mensagem = context.Message;
            
            var command = new SendDeliveryFailedEmailCommand(
                EventId: mensagem.EventId, 
                OrderId: mensagem.OrderId,
                UserId: mensagem.UserId,
                Reason: mensagem.Reason);

            await _sender.Send(command, context.CancellationToken);
        }
    }
}
