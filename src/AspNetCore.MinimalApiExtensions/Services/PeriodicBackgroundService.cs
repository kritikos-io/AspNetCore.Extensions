namespace Kritikos.AspNetCore.MinimalApiExtensions.Services;

using Kritikos.AspNetCore.MinimalApiExtensions.Extensions;
using Kritikos.AspNetCore.MinimalApiExtensions.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// A generic periodic background service that can be triggered externally.
/// </summary>
/// <remarks>Ensure you also call <see cref="OptionsConfigurationServiceCollectionExtensions.Configure{T}(IServiceCollection, string?, IConfiguration)"/> to register <typeparamref name="TOptions"/>.</remarks>
/// <param name="options">An implementation of <see cref="PeriodicBackgroundServiceOptions{T}"/> to provide needed parameters.</param>
/// <param name="logger">An <see cref="ILogger"/> instance to provide proper structured logs.</param>
/// <typeparam name="TService">The type of the service to be implemented (Curiously Recurring Template Pattern to overcome lack of the self keyword).</typeparam>
/// <typeparam name="TOptions">An implementation of <see cref="PeriodicBackgroundServiceOptions{T}"/> for <typeparamref name="TService"/>.</typeparam>
public abstract class PeriodicBackgroundService<TService, TOptions>(
  IOptions<TOptions> options,
  ILogger<TService> logger)
  : BackgroundService
  where TService : PeriodicBackgroundService<TService, TOptions>
  where TOptions : PeriodicBackgroundServiceOptions<TService>
{
  private readonly string serviceName = typeof(TService).Name;
  private readonly PeriodicTimer timer = new(options.Value.Interval);

  private CancellationTokenSource? cancellationTokenSource;

  protected TOptions Options { get; } = options.Value;

  protected ILogger Logger { get; } = logger;
  /// <inheritdoc />
  public override void Dispose()
  {
    base.Dispose();
    cancellationTokenSource?.Cancel();
    cancellationTokenSource?.Dispose();
    timer.Dispose();
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Triggers the background service to execute immediately.
  /// </summary>
  public void Trigger() => cancellationTokenSource?.Cancel();

  /// <inheritdoc />
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        Logger.LogPeriodicBackgroundServiceSleeping(serviceName);
        RefreshCancellationToken(stoppingToken);
        await timer.WaitForNextTickAsync(cancellationTokenSource!.Token);
      }
      catch (OperationCanceledException)
      {
        if (stoppingToken.IsCancellationRequested)
        {
          break;
        }

        Logger.LogPeriodicBackgroundServiceTriggeredManually(serviceName);
      }

      try
      {
        if ((cancellationTokenSource?.IsCancellationRequested ?? false)
            && Options.TriggerStopsCurrentExecution)
        {
          continue;
        }

        RefreshCancellationToken(stoppingToken);

        Logger.LogPeriodicBackgroundServiceTriggered(serviceName);
        await DoWork(
                Options.TriggerStopsCurrentExecution
                    ? cancellationTokenSource!.Token
                    : stoppingToken);
      }
      catch (OperationCanceledException)
      {
      }
#pragma warning disable CA1031 // Background service should not throw exceptions
      catch (Exception e)
#pragma warning restore CA1031
      {
        Logger.LogUnhandledException(e);
      }
    }
  }

  /// <summary>
  /// The actual work to be done by the background service.
  /// </summary>
  /// <param name="stoppingToken">A <see cref="CancellationToken"/> to notify the service when it is time to shut down.</param>
  /// <returns>A <see cref="Task"/> that represents the asynchronous Start operation.</returns>
  /// <exception cref=""></exception>
  protected abstract Task DoWork(CancellationToken stoppingToken);

  private void RefreshCancellationToken(CancellationToken stoppingToken)
  {
    if (cancellationTokenSource is { Token.IsCancellationRequested: false })
    {
      return;
    }

    cancellationTokenSource?.Dispose();
    cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
  }
}
