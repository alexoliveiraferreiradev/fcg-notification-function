using System.Text.Json;

namespace Fcg.Notification.Function.Infrastructure.MessageBroker.Messages
{
    public static class EnvelopeReader
    {
        private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

        public static T Read<T>(string body) where T : class
        => JsonSerializer.Deserialize<MassTransitEnvelope<T>>(body, Options)?.Message
           ?? throw new InvalidOperationException($"Mensagem inválida para {typeof(T).Name}.");
    }
}
