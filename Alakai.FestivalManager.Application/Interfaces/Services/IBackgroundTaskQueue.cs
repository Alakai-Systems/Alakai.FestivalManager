namespace Alakai.FestivalManager.Application.Interfaces.Services;

/// <summary>
/// Cola de trabajo en segundo plano ligada al ciclo de vida de la aplicacion.
/// A diferencia de un Task.Run suelto, el trabajo encolado aqui sobrevive a
/// que la peticion HTTP que lo encolo ya haya terminado - solo se pierde si
/// la aplicacion entera se apaga.
/// </summary>
public interface IBackgroundTaskQueue
{
    void QueueBackgroundWorkItem(Func<IServiceProvider, CancellationToken, Task> workItem);
    Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}