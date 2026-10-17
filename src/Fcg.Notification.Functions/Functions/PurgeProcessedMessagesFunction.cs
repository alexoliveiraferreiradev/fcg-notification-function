using Fcg.Notification.Function.Application.Ports;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Fcg.Notification.FunctionApp.Functions;

/// <summary>
/// Faz o papel do TTL que o Redis aplicava nas chaves de idempotência.
/// </summary>
public class PurgeProcessedMessagesFunction
{
    private readonly ILogger _logger;
    private readonly IIdempotencyService _idempotencyService;
    public PurgeProcessedMessagesFunction(ILoggerFactory loggerFactory, IIdempotencyService idempotencyService)
    {
        _logger = loggerFactory.CreateLogger<PurgeProcessedMessagesFunction>();
        _idempotencyService = idempotencyService;
    }

    [Function(nameof(PurgeProcessedMessagesFunction))]
    public async Task Run([TimerTrigger("0 0 3 * * *")] TimerInfo timer, CancellationToken cancellationToken)
    {
        var removed = await _idempotencyService.PurgeExpiredAsync(cancellationToken);
        _logger.LogInformation("TimerTrigger: {Removed} chaves de idempotência expiradas removidas.", removed);
    }
}
