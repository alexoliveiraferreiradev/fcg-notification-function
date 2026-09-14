using System;
using System.Text.Json;
using Fcg.Notification.Function.Application.UseCase.ApprovedPaymentEmail;
using Fcg.Notification.Function.Infrastructure.MessageBroker;
using Fcg.Notification.Function.Infrastructure.MessageBroker.Messages;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Fcg.Notification.FunctionApp.Functions;

public class SendApprovedPaymentEmailFunction
{
    private readonly ILogger _logger;
    private readonly ISender _sender;

    public SendApprovedPaymentEmailFunction(ILoggerFactory loggerFactory, ISender sender)
    {
        _logger = loggerFactory.CreateLogger<SendApprovedPaymentEmailFunction>();
        _sender = sender;
    }

    [Function(nameof(SendApprovedPaymentEmailFunction))]
    public async Task Run([RabbitMQTrigger(NotificationRabbitTopology.PaymentProcessedQueue,
        ConnectionStringSetting = "RabbitMqConnection")] string message)
    {
        var envelope = JsonSerializer.Deserialize<MassTransitEnvelope<PaymentProcessedIntegrationEvent>>(message,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var evt = envelope?.Message;

        if(evt is null)
        {
            throw new InvalidOperationException($"Falha ao deserializar mensagem: {message}");
        }

        await _sender.Send(new SendPaymentApprovedEmailCommand(evt.EventId,evt.UserId,evt.OrderId, evt.CreatedAt)); 

        _logger.LogInformation("RabbitMqTrigger: Function Send Approved Payment Email processada: {item}", message);
    }
}