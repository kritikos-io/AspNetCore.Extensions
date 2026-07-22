namespace Kritikos.AspNetCore.MinimalApiExtensions.Services.Heartbeat;

using Kritikos.AspNetCore.MinimalApiExtensions.Extensions;
using Kritikos.AspNetCore.MinimalApiExtensions.Options;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Background watchdog that periodically scans the <see cref="HeartbeatRegistry"/> and cancels the
/// <see cref="Heartbeat.Token"/> of every source whose most recent beat is older than its effective timeout,
/// so stalled operations that linked the token abort. The scan cadence follows
/// <see cref="HeartbeatOptions.WatchdogInterval"/> and is updated live when that option changes; the staleness
/// window is read fresh on each scan from <see cref="HeartbeatOptions.DefaultTimeout"/>.
/// </summary>
/// <param name="registry">Registry scanned for stale sources.</param>
/// <param name="timeProvider">Clock driving the scan timer and the staleness comparison.</param>
/// <param name="options">Monitor supplying the scan interval and the default staleness window.</param>
/// <param name="logger">Logger used to record cancelled sources.</param>
public sealed class HeartbeatWatchdog(
  HeartbeatRegistry registry,
  TimeProvider timeProvider,
  IOptionsMonitor<HeartbeatOptions> options,
  ILogger<HeartbeatWatchdog> logger)
  : BackgroundService
{
  /// <summary>Cancels every source whose beat is stale as of now and logs each cancellation and failure.</summary>
  internal void Scan()
  {
    using var activity = Telemetry.ActivitySource.StartActivity("ScanHeartbeats");
    var outcome = registry.CancelStaleSources(options.CurrentValue.DefaultTimeout);

    foreach (var name in outcome.Cancelled)
    {
      logger.LogHeartbeatSourceCancelled(name);
    }

    foreach (var (source, error) in outcome.Failures)
    {
      logger.LogHeartbeatSourceCancelFailed(source, error);
    }

    if (outcome.Cancelled.Count > 0)
    {
      Telemetry.SourcesCancelled.Add(outcome.Cancelled.Count);
    }

    if (outcome.Failures.Count > 0)
    {
      Telemetry.CancellationsFailed.Add(outcome.Failures.Count);
    }

    activity?.SetTag("kritikos.heartbeat.cancelled_count", outcome.Cancelled.Count);
    activity?.SetTag("kritikos.heartbeat.failed_count", outcome.Failures.Count);
  }

  /// <inheritdoc />
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    using var timer = new PeriodicTimer(options.CurrentValue.WatchdogInterval, timeProvider);
    using var reload = options.OnChange(changed =>
    {
      try
      {
        if (changed.WatchdogInterval > TimeSpan.Zero)
        {
          timer.Period = changed.WatchdogInterval;
        }
      }
      catch (ObjectDisposedException)
      {
        // The watchdog is shutting down and the timer is gone; the new cadence no longer matters.
      }
    });

    while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
    {
      Scan();
    }
  }
}
