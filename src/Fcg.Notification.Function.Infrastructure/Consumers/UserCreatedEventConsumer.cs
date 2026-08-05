using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Notification.Function.Application.UseCase.WelcomeEmail;
using MassTransit;
using MediatR;

namespace Fcg.Notification.Function.Infrastructure.Consumers
{
    public class UserCreatedEventConsumer : IConsumer<UserCreatedEvent>
    {        
        private readonly ISender _sender;

        public UserCreatedEventConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var message = context.Message;       
            
            await _sender.Send(new RegisterUserCommand(
                message.EventId,message.UserId, message.Name, message.Email, message.CreatedAt),
                context.CancellationToken);    

            await _sender.Send(new SendWelcomeEmailCommand(
                message.EventId,message.UserId,message.Email,message.Name),
                context.CancellationToken);
        }
    }
}
