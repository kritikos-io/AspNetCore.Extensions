namespace Kritikos.AspNetCore.MinimalApiExtensions.Services.Heartbeat;

using System.Collections.Concurrent;

/// <summary>
/// Thread-safe registry of named heartbeat sources. Long-running services <see cref="Register"/> a source
/// (which seeds an initial beat), periodically <see cref="Heartbeat.Beat"/> it, and dispose the returned
/// handle to unregister on shutdown. The aggregate heartbeat health check reports unhealthy when any
/// registered source's most recent beat is older than its timeout.
/// </summary>
public sealed class HeartbeatRegistry
{
  private readonly ConcurrentDictionary<string, Heartbeat> sources = new(StringComparer.Ordinal);
  private readonly TimeProvider timeProvider;

  /// <summary>Initializes a new instance of the <see cref="HeartbeatRegistry"/> class.</summary>
  /// <param name="timeProvider">Clock abstraction used to timestamp beats.</param>
  public HeartbeatRegistry(TimeProvider timeProvider)
  {
    this.timeProvider = timeProvider;
    Telemetry.BindActiveSourceCount(() => sources.Count);
  }

  /// <summary>Registers a heartbeat source, seeding an initial beat so it starts fresh and tracked.</summary>
  /// <param name="name">Unique source name; re-registering disposes and replaces the existing source.</param>
  /// <param name="timeout">Per-source staleness window, or <see langword="null"/> to use the configured default.</param>
  /// <returns>A disposable handle the caller ticks to record progress and disposes to unregister.</returns>
  public Heartbeat Register(string name, TimeSpan? timeout = null)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    var heartbeat = new Heartbeat(this, timeProvider, name, timeout);
    Heartbeat? previous = null;
    sources.AddOrUpdate(
      name,
      heartbeat,
      (_, existing) =>
      {
        previous = existing;
        return heartbeat;
      });
    previous?.Dispose();
    return heartbeat;
  }

  /// <summary>Captures the current state of every registered source.</summary>
  /// <returns>A point-in-time snapshot of all sources.</returns>
  internal IReadOnlyList<HeartbeatSnapshot> Snapshot()
    => [.. sources.Select(pair => pair.Value.ToSnapshot())];

  /// <summary>Removes a source, but only if <paramref name="heartbeat"/> is still the registered handle.</summary>
  /// <param name="heartbeat">The handle whose source to remove.</param>
  internal void Unregister(Heartbeat heartbeat)
    => sources.TryRemove(new KeyValuePair<string, Heartbeat>(heartbeat.Name, heartbeat));

  /// <summary>Cancels the token of every source whose most recent beat is older than its effective timeout.</summary>
  /// <param name="defaultTimeout">Timeout applied to sources without their own.</param>
  /// <returns>The sources cancelled by this call and those whose cancellation threw.</returns>
  /// <remarks>
  /// A source's cancellation runs consumer callbacks synchronously, so a throwing callback surfaces here as an
  /// exception. Each source is guarded independently so one failure neither aborts the scan nor faults the
  /// watchdog loop that would otherwise stop the host; failures are reported for the caller to log.
  /// </remarks>
  internal HeartbeatScanOutcome CancelStaleSources(TimeSpan defaultTimeout)
  {
    List<string>? cancelled = null;
    List<(string Source, Exception Error)>? failures = null;
    foreach (var pair in sources)
    {
      try
      {
        if (pair.Value.CancelIfStale(defaultTimeout))
        {
          (cancelled ??= []).Add(pair.Key);
        }
      }
#pragma warning disable CA1031 // Watchdog must survive a throwing cancellation callback
      catch (Exception e)
#pragma warning restore CA1031
      {
        (failures ??= []).Add((Source: pair.Key, Error: e));
      }
    }

    return new HeartbeatScanOutcome(cancelled ?? [], failures ?? []);
  }
}
