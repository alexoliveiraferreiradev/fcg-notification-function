using System;
using System.Text.Json;
using Fcg.Core.SharedContracts.Messaging;
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
    public async Task Run([ServiceBusTrigger(Topics.PaymentApproved, Topics.NotificationSubscription, Connection = NotificationBusTopology.ConnectionName)] string message)
    {
        var envelope = EnvelopeReader.Read<PaymentProcessedIntegrationEvent>(message);
        
        if(envelope is null)
        {
            throw new InvalidOperationException($"Falha ao deserializar mensagem: {message}");
        }

        await _sender.Send(new SendPaymentApprovedEmailCommand(envelope.EventId, envelope.UserId, envelope.OrderId, envelope.CreatedAt)); 

        _logger.LogInformation("ServiceBusTrigger: Function Send Approved Payment Email processada: {item}", message);
    }
}