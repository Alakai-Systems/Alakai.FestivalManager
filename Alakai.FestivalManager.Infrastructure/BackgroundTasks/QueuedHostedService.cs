using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alakai.FestivalManager.Infrastructure.BackgroundTasks;

/// <summary>
/// Procesa la cola de IBackgroundTaskQueue mientras la aplicacion este viva.
/// Cada item se ejecuta en su PROPIO scope de DI (nunca reutiliza el scope de
/// la peticion HTTP que lo encolo, que ya estara cerrado para entonces).
/// </summary>
public class QueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QueuedHostedService> _logger;

    public QueuedHostedService(IBackgroundTaskQueue taskQueue, IServiceScopeFactory scopeFactory, ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Func<IServiceProvider, CancellationToken, Task> workItem;

            try
            {
                workItem = await _taskQueue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                await workItem(scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing queued background work item.");
            }
        }
    }
}