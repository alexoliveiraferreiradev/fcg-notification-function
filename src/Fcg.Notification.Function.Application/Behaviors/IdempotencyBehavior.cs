using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Application.Ports;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Notification.Function.Application.Behaviors
{
    public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IIdempotentCommand
    {
        private readonly IIdempotencyService _idempotencyService;
        private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger;

        public IdempotencyBehavior(IIdempotencyService idempotencyService, ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
        {
            _idempotencyService = idempotencyService;
            _logger = logger;
        }

       public async Task<TResponse> Handle(
           TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var canProcess = await _idempotencyService.TryProcessAsync(request.EventId);

            if(!canProcess)
            {
                _logger.LogInformation("Evento {EventId} já foi processado. Ignorando execução.", request.EventId);
                return typeof(TResponse) == typeof(Unit) 
                    ? (TResponse)(object)Unit.Value
                    : default!;
            }

            if (!await _idempotencyService.TryProcessAsync(request.EventId))
                return default!; 

            try
            {
                return await next(cancellationToken);
            }
            catch
            {
                await _idempotencyService.ReleaseAsync(request.EventId);
                throw;
            }
        }
    }
}
