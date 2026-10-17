using Fcg.Core.Abstractions.Interfaces;
using Fcg.Notification.Function.Application.Common.Exceptions;
using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Application.Ports;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Notification.Function.Application.Behaviors
{
    /// <summary>
    /// Entrega at-least-once: a chave é gravada junto com a notificação, depois do envio do email.
    /// Se duas entregas da mesma mensagem correrem em paralelo, as duas podem enviar o email;
    /// a segunda perde no commit (PK da chave) e é tratada como já processada.
    /// </summary>
    public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IIdempotentCommand
    {
        private readonly IIdempotencyService _idempotencyService;
        private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public IdempotencyBehavior(IIdempotencyService idempotencyService,
            IUnitOfWork unitOfWork,
            ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
        {
            _idempotencyService = idempotencyService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var idempotencyKey = $"{typeof(TRequest).Name}:{request.EventId}";
            var canProcess = await _idempotencyService.TryProcessAsync(idempotencyKey, cancellationToken);

            if (!canProcess)
            {
                _logger.LogInformation("Evento {EventId} já processado por {CommandType}. Ignorando.", request.EventId, typeof(TRequest).Name);
                return EmptyResponse();
            }

            try
            {
                var response = await next(cancellationToken);
                await _unitOfWork.CommitAsync();
                return response;
            }
            catch (DuplicateMessageException)
            {
                // Pode vir do commit do próprio handler ou do commit acima.
                _logger.LogWarning("Evento {EventId} foi processado em paralelo por outra entrega de {CommandType}. Ignorando.", request.EventId, typeof(TRequest).Name);
                return EmptyResponse();
            }
        }

        private static TResponse EmptyResponse() =>
            typeof(TResponse) == typeof(Unit) ? (TResponse)(object)Unit.Value : default!;
    }
}
