using System;
using System.Text.Json;
using Fcg.Notification.Function.Application.UseCase.DeliveryFailedEmail;
using Fcg.Notification.Function.Infrastructure.MessageBroker;
using Fcg.Notification.Function.Infrastructure.MessageBroker.Messages;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Fcg.Notification.FunctionApp.Functions;

public class SendDeliveryEmailFunction
{
    private readonly ILogger _logger;
    private readonly ISender _sender;
    public SendDeliveryEmailFunction(ILoggerFactory loggerFactory, ISender sender)
    {
        _logger = loggerFactory.CreateLogger<SendDeliveryEmailFunction>();
        _sender = sender;
    }

    [Function(nameof(SendDeliveryEmailFunction))]
    public async Task Run([RabbitMQTrigger(NotificationRabbitTopology.DeliveryFailedQueue,
        ConnectionStringSetting = "RabbitMqConnection")] string message)
    {
        var envelope = JsonSerializer.Deserialize<MassTransitEnvelope<DeliveryFailedIntegrationEvent>>(message,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true});

        var evt = envelope?.Message;

        if(evt is null)
        {
            throw new InvalidOperationException($"Falha ao deserializar mensagem: {message}");
        }

        await _sender.Send(new SendDeliveryFailedEmailCommand(evt.EventId, evt.OrderId, evt.UserId, evt.Reason));
        _logger.LogInformation("RabbitMqTrigger: Função Send Delivery Email processada: {item}", message);
    }
}