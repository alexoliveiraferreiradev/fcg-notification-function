using System;
using System.Text.Json;
using Fcg.Core.SharedContracts.Messaging;
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
    public async Task Run([ServiceBusTrigger(Topics.PurchaseRejected, Topics.NotificationSubscription, Connection = NotificationBusTopology.ConnectionName)] string message)
    {
        var envelope = EnvelopeReader.Read<PaymentFailedIntegrationEvent>(message);

        if(envelope is null)
        {
            throw new InvalidOperationException($"Falha ao deserializar mensagem: {message}");
        }

        await _sender.Send(new SendPaymentRejectCommand(envelope.EventId, envelope.OrderId, envelope.UserId, envelope.Reason));
        _logger.LogInformation("ServiceBusTrigger: Função Send Failed Payment processada: {item}", message);
    }
}