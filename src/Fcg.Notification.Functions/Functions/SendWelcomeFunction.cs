using Fcg.Core.SharedContracts.Messaging;
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
    public async Task Run([ServiceBusTrigger(Topics.UserCreated, Topics.NotificationSubscription, Connection = NotificationBusTopology.ConnectionName)] string message)
    {
        var envelope = EnvelopeReader.Read<UserCreatedIntegrationEvent>(message);
       
        if(envelope is null)
        {
            throw new InvalidOperationException($"Falha ao deserializar mensagem: {message}");
        }

        await _sender.Send(new RegisterUserCommand(envelope.EventId, envelope.UserId, envelope.Name, envelope.Email, envelope.CreatedAt));
        await _sender.Send(new SendWelcomeEmailCommand(envelope.EventId, envelope.UserId, envelope.Email, envelope.Name));
        _logger.LogInformation("ServiceBusTrigger: Send Welcome function processada: {item}", envelope);
    }
}