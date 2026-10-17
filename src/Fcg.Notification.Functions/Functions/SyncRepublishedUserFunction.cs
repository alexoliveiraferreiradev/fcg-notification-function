using Fcg.Core.SharedContracts.Messaging;
using Fcg.Notification.Function.Application.UseCase.WelcomeEmail;
using Fcg.Notification.Function.Infrastructure.MessageBroker;
using Fcg.Notification.Function.Infrastructure.MessageBroker.Messages;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Fcg.Notification.FunctionApp.Functions;

/// <summary>
/// Sincroniza o UserSnapshot a partir de um usuário republicado pela API de usuários.
/// Diferente do SendWelcomeFunction, não envia e-mail: serve para recuperar usuários
/// que não chegaram à function no momento do cadastro.
/// </summary>
public class SyncRepublishedUserFunction
{
    private readonly ILogger _logger;
    private readonly ISender _sender;

    public SyncRepublishedUserFunction(ILoggerFactory loggerFactory, ISender sender)
    {
        _logger = loggerFactory.CreateLogger<SyncRepublishedUserFunction>();
        _sender = sender;
    }

    [Function(nameof(SyncRepublishedUserFunction))]
    public async Task Run([ServiceBusTrigger(Topics.UserRepublished, Topics.NotificationSubscription, Connection = NotificationBusTopology.ConnectionName)] string message)
    {
        var envelope = EnvelopeReader.Read<RepublishUserIntegrationEvent>(message);

        if (envelope is null)
        {
            throw new InvalidOperationException($"Falha ao deserializar mensagem: {message}");
        }

        await _sender.Send(new RegisterUserCommand(envelope.EventId, envelope.UserId, envelope.Name, envelope.Email, envelope.CreatedAt));
        _logger.LogInformation("ServiceBusTrigger: Sync Republished User function processada: {item}", envelope);
    }
}
