using System;
using System.Text.Json;
using Fcg.Notification.Function.Application.UseCase.PaymentRejectEmail;
using Fcg.Notification.Function.Infrastructure.MessageBroker;
using Fcg.Notification.Function.Infrastructure.MessageBroker.Messages;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Fcg.Notification.FunctionApp.Functions;

public class SendFailedPaymentEmailFunction
{
    private readonly ILogger _logger;
    private readonly ISender _sender;

    public SendFailedPaymentEmailFunction(ILoggerFactory loggerFactory, ISender sender)
    {
        _logger = loggerFactory.CreateLogger<SendFailedPaymentEmailFunction>();
        _sender = sender;
    }

    [Function(nameof(SendFailedPaymentEmailFunction))]
    public async Task Run([RabbitMQTrigger(NotificationRabbitTopology.PaymentFailedQueue,
        ConnectionStringSetting = "RabbitMqConnection")] string message)
    {
        var envelope = JsonSerializer.Deserialize<MassTransitEnvelope<PaymentFailedIntegrationEvent>>(message,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true});

        var evt = envelope?.Message;

        if(evt is null)
        {
            throw new InvalidOperationException($"Falha ao deserializar mensagem: {message}");
        }

        await _sender.Send(new SendPaymentRejectCommand(evt.EventId, evt.OrderId, evt.UserId, evt.Reason));
        _logger.LogInformation("RabbitMqTrigger: Função Send Failed Payment processada: {item}", message);
    }
}