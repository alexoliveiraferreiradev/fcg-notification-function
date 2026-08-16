using Fcg.Notification.Function.Application.UseCase.WelcomeEmail;
using Fcg.Notification.Function.Infrastructure.MessageBroker;
using Fcg.Notification.Function.Infrastructure.MessageBroker.Messages;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Fcg.Notification.FunctionApp.Functions;

public class SendWelcomeFunction
{
    private readonly ILogger _logger;
    private readonly ISender _sender;

    public SendWelcomeFunction(ILoggerFactory loggerFactory, ISender sender)
    {
        _logger = loggerFactory.CreateLogger<SendWelcomeFunction>();
        _sender = sender;
    }

    [Function(nameof(SendWelcomeFunction))]
    public async Task Run([RabbitMQTrigger(NotificationRabbitTopology.UserCreatedQueue, ConnectionStringSetting = "RabbitMqConnection")] string message)
    {
        var envelope = JsonSerializer.Deserialize<MassTransitEnvelope<UserCreatedIntegrationEvent>>(message, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true});

       
        var evt = envelope?.Message;

        if(evt is null)
        {
            throw new InvalidOperationException($"Falha ao deserializar mensagem: {message}");
        }

        await _sender.Send(new RegisterUserCommand(evt.EventId,evt.UserId,evt.Name,evt.Email,evt.CreatedAt));
        await _sender.Send(new SendWelcomeEmailCommand(evt.EventId, evt.UserId, evt.Email, evt.Name));
        _logger.LogInformation("RabbitMqTrigger: Send Welcome function processada: {item}", evt);
    }
}