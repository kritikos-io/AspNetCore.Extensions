namespace Kritikos.AspNetCore.MinimalApiExtensions.Services.Heartbeat;

/// <summary>Immutable view of a registered heartbeat source, consumed by the aggregate health check.</summary>
/// <param name="Name">The source name.</param>
/// <param name="LastBeat">Wall-clock timestamp of the source's most recent beat, for display.</param>
/// <param name="Age">Monotonic time elapsed since the most recent beat, used for staleness decisions.</param>
/// <param name="Timeout">Per-source staleness window, or <see langword="null"/> to use the configured default.</param>
/// <param name="IsCancelled">Whether the watchdog has already cancelled the source.</param>
public readonly record struct HeartbeatSnapshot(
  string Name,
  DateTimeOffset LastBeat,
  TimeSpan Age,
  TimeSpan? Timeout,
  bool IsCancelled);
