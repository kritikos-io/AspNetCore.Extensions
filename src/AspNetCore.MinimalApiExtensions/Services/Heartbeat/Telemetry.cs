namespace Kritikos.AspNetCore.MinimalApiExtensions.Services.Heartbeat;

using System.Diagnostics;
using System.Diagnostics.Metrics;

/// <summary>
/// Diagnostics for the heartbeat component: one <see cref="System.Diagnostics.ActivitySource"/> and one
/// <see cref="System.Diagnostics.Metrics.Meter"/>, both named <see cref="Name"/>. Wire that name into a
/// collector such as OpenTelemetry to observe the instruments; with no listener attached they are effectively
/// free. Built on <c>System.Diagnostics</c> only, so it adds no dependency.
/// </summary>
public static class Telemetry
{
  /// <summary>The shared name of the heartbeat activity source and meter.</summary>
  public const string Name = "Kritikos.AspNetCore.MinimalApiExtensions.Heartbeat";

  /// <summary>Traces each watchdog scan.</summary>
  internal static readonly ActivitySource ActivitySource;

  /// <summary>Counts sources the watchdog cancelled for exceeding their timeout.</summary>
  internal static readonly Counter<long> SourcesCancelled;

  /// <summary>Counts cancellations that threw from a linked-token callback.</summary>
  internal static readonly Counter<long> CancellationsFailed;

  private static readonly Meter Meter;

  private static Func<int>? activeSourceProvider;

  static Telemetry()
  {
    Meter = new Meter(Name);
    ActivitySource = new ActivitySource(Name);
    SourcesCancelled = Meter.CreateCounter<long>(
      "heartbeat_sources_cancelled_total",
      unit: "{source}",
      description: "Heartbeat sources cancelled by the watchdog for exceeding their timeout.");
    CancellationsFailed = Meter.CreateCounter<long>(
      "heartbeat_cancellations_failed_total",
      unit: "{failure}",
      description: "Heartbeat source cancellations that threw from a linked-token callback.");
    Meter.CreateObservableGauge(
      "heartbeat_sources_active",
      ObserveActiveSources,
      unit: "{source}",
      description: "Heartbeat sources currently registered.");
  }

  /// <summary>Binds the live registered-source count into the active-sources gauge.</summary>
  /// <param name="provider">Reads the current number of registered sources.</param>
  internal static void BindActiveSourceCount(Func<int> provider) => activeSourceProvider = provider;

  private static IEnumerable<Measurement<int>> ObserveActiveSources()
    => activeSourceProvider is { } provider ? [new Measurement<int>(provider())] : [];
}
