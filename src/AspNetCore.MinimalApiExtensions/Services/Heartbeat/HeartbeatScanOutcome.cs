namespace Kritikos.AspNetCore.MinimalApiExtensions.Services.Heartbeat;

/// <summary>
/// Result of a single watchdog scan over the <see cref="HeartbeatRegistry"/>: the sources whose token was
/// cancelled, and the sources whose cancellation threw. Separating the two lets a misbehaving cancellation
/// callback be logged without faulting the watchdog loop and stopping the host.
/// </summary>
/// <param name="Cancelled">Names of the sources this scan transitioned to cancelled.</param>
/// <param name="Failures">Sources whose cancellation raised an exception, paired with that exception.</param>
internal readonly record struct HeartbeatScanOutcome(
  IReadOnlyList<string> Cancelled,
  IReadOnlyList<(string Source, Exception Error)> Failures);
