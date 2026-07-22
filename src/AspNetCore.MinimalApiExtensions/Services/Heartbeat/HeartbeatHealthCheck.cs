namespace Kritikos.AspNetCore.MinimalApiExtensions.Services.Heartbeat;

using Kritikos.AspNetCore.MinimalApiExtensions.Options;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

/// <summary>
/// Aggregate liveness health check over every source registered with the <see cref="HeartbeatRegistry"/>.
/// Reports the registration's failure status when any source has been cancelled by the watchdog or its
/// monotonic age exceeds its effective timeout (its own <see cref="HeartbeatSnapshot.Timeout"/>, falling back
/// to <see cref="HeartbeatOptions.DefaultTimeout"/>); otherwise healthy. A process with no registered sources
/// is treated as healthy so the probe never restarts an idle-but-alive host.
/// </summary>
/// <param name="registry">Registry whose sources are inspected.</param>
/// <param name="options">Monitor supplying the default staleness window.</param>
public sealed class HeartbeatHealthCheck(
  HeartbeatRegistry registry,
  IOptionsMonitor<HeartbeatOptions> options)
  : IHealthCheck
{
  /// <summary>The registration name of the heartbeat health check.</summary>
  public const string Name = "heartbeat";

  /// <inheritdoc />
  public Task<HealthCheckResult> CheckHealthAsync(
    HealthCheckContext context,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);

    var defaultTimeout = options.CurrentValue.DefaultTimeout;
    var sources = registry.Snapshot();

    var stale = new List<string>();
    var data = new Dictionary<string, object>(StringComparer.Ordinal);
    foreach (var source in sources)
    {
      data[source.Name] = source.LastBeat;
      if (source.IsCancelled || source.Age > (source.Timeout ?? defaultTimeout))
      {
        stale.Add(source.Name);
      }
    }

    var result = stale.Count > 0
      ? new HealthCheckResult(
        context.Registration.FailureStatus,
        $"Stale heartbeat sources: {string.Join(", ", stale)}.",
        data: data)
      : HealthCheckResult.Healthy(
        sources.Count == 0 ? "No heartbeat sources registered." : "All heartbeat sources are fresh.",
        data);

    return Task.FromResult(result);
  }
}
