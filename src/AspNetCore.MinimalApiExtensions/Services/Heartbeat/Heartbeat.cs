namespace Kritikos.AspNetCore.MinimalApiExtensions.Services.Heartbeat;

/// <summary>
/// Handle to a registered heartbeat source. A service ticks it via <see cref="Beat"/> to signal progress,
/// links <see cref="Token"/> into its cancellable work so the watchdog can abort a stalled operation, and
/// disposes it to unregister the source (e.g. on graceful shutdown). Members are thread-safe.
/// </summary>
/// <remarks>
/// Ownership follows the handle: the registering service must dispose it when the work ends. A source that the
/// watchdog has cancelled (bitten), or that outlives its work without being disposed, keeps the liveness probe
/// unhealthy, because <see cref="Beat"/> no longer refreshes a cancelled source. Recovery is a re-register (to
/// obtain a fresh token), not a beat.
/// </remarks>
public sealed class Heartbeat : IDisposable
{
  private readonly HeartbeatRegistry registry;
  private readonly TimeProvider timeProvider;
  private readonly CancellationTokenSource cancellation = new();
  private readonly Lock gate = new();
  private long lastBeatTimestamp;
  private long lastBeatUtcTicks;
  private bool disposed;

  /// <summary>Initializes a new instance of the <see cref="Heartbeat"/> class and seeds its first beat.</summary>
  /// <param name="registry">The owning registry, used to unregister on dispose.</param>
  /// <param name="timeProvider">Clock used to timestamp beats.</param>
  /// <param name="name">The source name.</param>
  /// <param name="timeout">Per-source staleness window, or <see langword="null"/> to use the configured default.</param>
  internal Heartbeat(HeartbeatRegistry registry, TimeProvider timeProvider, string name, TimeSpan? timeout)
  {
    this.registry = registry;
    this.timeProvider = timeProvider;
    Name = name;
    Timeout = timeout;
    lastBeatTimestamp = timeProvider.GetTimestamp();
    lastBeatUtcTicks = timeProvider.GetUtcNow().UtcTicks;
  }

  /// <summary>
  /// Gets a token the watchdog cancels when this source goes stale. Link it into cancellable work so a
  /// stalled operation aborts; the token is one-shot, so re-register to obtain a fresh one after a bite.
  /// </summary>
  public CancellationToken Token => cancellation.Token;

  /// <summary>Gets the source name.</summary>
  internal string Name { get; }

  /// <summary>Gets the per-source staleness window, or <see langword="null"/> to use the configured default.</summary>
  internal TimeSpan? Timeout { get; }

  /// <summary>Gets the wall-clock timestamp of the most recent beat, for display in the health data.</summary>
  internal DateTimeOffset LastBeat => new(Interlocked.Read(ref lastBeatUtcTicks), TimeSpan.Zero);

  /// <summary>Gets the monotonic time elapsed since the most recent beat, used for staleness decisions.</summary>
  internal TimeSpan Elapsed => timeProvider.GetElapsedTime(Interlocked.Read(ref lastBeatTimestamp));

  /// <summary>Gets a value indicating whether the watchdog has already cancelled this source.</summary>
  internal bool IsCancelled => cancellation.IsCancellationRequested;

  /// <summary>Records a beat at the current instant. A cancelled or disposed source is left untouched.</summary>
  public void Beat()
  {
    lock (gate)
    {
      if (disposed || cancellation.IsCancellationRequested)
      {
        return;
      }

      Interlocked.Exchange(ref lastBeatTimestamp, timeProvider.GetTimestamp());
      Interlocked.Exchange(ref lastBeatUtcTicks, timeProvider.GetUtcNow().UtcTicks);
    }
  }

  /// <summary>Unregisters the source and releases its token. Safe to call more than once.</summary>
  public void Dispose()
  {
    lock (gate)
    {
      if (disposed)
      {
        return;
      }

      disposed = true;
    }

    registry.Unregister(this);
    cancellation.Dispose();
  }

  /// <summary>Captures this source's current state as an immutable snapshot for the health check.</summary>
  /// <returns>A point-in-time view of the source.</returns>
  internal HeartbeatSnapshot ToSnapshot() => new(Name, LastBeat, Elapsed, Timeout, IsCancelled);

  /// <summary>
  /// Cancels the source's <see cref="Token"/> when the monotonic time since its most recent beat exceeds the
  /// effective timeout. Returns <see langword="true"/> only on the transition to cancelled.
  /// </summary>
  /// <param name="defaultTimeout">Timeout applied when the source has none of its own.</param>
  /// <returns>Whether this call cancelled the source.</returns>
  /// <remarks>
  /// Staleness is decided under the <c>gate</c> lock, but the cancellation itself runs outside it: cancellation
  /// callbacks execute synchronously, so holding the lock across them would let a slow callback stall the scan
  /// and let a re-entrant callback deadlock the non-reentrant lock. A concurrent dispose is treated as benign.
  /// </remarks>
  internal bool CancelIfStale(TimeSpan defaultTimeout)
  {
    lock (gate)
    {
      if (disposed || cancellation.IsCancellationRequested || Elapsed <= (Timeout ?? defaultTimeout))
      {
        return false;
      }
    }

    try
    {
      cancellation.Cancel();
    }
    catch (ObjectDisposedException)
    {
      return false;
    }

    return true;
  }
}
