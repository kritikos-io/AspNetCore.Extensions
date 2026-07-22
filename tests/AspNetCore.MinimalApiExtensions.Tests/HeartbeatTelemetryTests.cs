namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using System.Diagnostics;
using System.Diagnostics.Metrics;

using Kritikos.AspNetCore.MinimalApiExtensions.Options;
using Kritikos.AspNetCore.MinimalApiExtensions.Services.Heartbeat;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NSubstitute;

public sealed class HeartbeatTelemetryTests
{
  private static readonly DateTimeOffset Start = DateTimeOffset.UnixEpoch;

  [Test]
  [NotInParallel]
  public async Task Scan_records_a_cancellation_metric_and_a_span()
  {
    var cancellations = 0L;
    using var meterListener = new MeterListener();
    meterListener.InstrumentPublished = (instrument, listener) =>
    {
      if (instrument.Meter.Name == Telemetry.Name && instrument.Name == "heartbeat_sources_cancelled_total")
      {
        listener.EnableMeasurementEvents(instrument);
      }
    };
    meterListener.SetMeasurementEventCallback<long>((_, measurement, _, _) => cancellations += measurement);
    meterListener.Start();

    var spans = new List<Activity>();
    using var activityListener = new ActivityListener
    {
      ShouldListenTo = source => source.Name == Telemetry.Name,
      Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
      ActivityStopped = spans.Add,
    };
    ActivitySource.AddActivityListener(activityListener);

    var clock = Clock();
    var registry = new HeartbeatRegistry(clock);
    registry.Register("walker", TimeSpan.FromSeconds(5));
    var watchdog = Watchdog(registry, clock);
    clock.GetTimestamp().Returns(TimeSpan.FromSeconds(6).Ticks);

    watchdog.Scan();

    await Assert.That(cancellations).IsGreaterThanOrEqualTo(1);
    await Assert.That(spans.Any(span => span.OperationName == "ScanHeartbeats")).IsTrue();
  }

  [Test]
  [NotInParallel]
  public async Task Active_sources_gauge_reports_the_registered_count()
  {
    var measurements = new List<int>();
    var registry = new HeartbeatRegistry(Clock());
    registry.Register("one");
    registry.Register("two");
    registry.Register("three");

    using var meterListener = new MeterListener();
    meterListener.InstrumentPublished = (instrument, listener) =>
    {
      if (instrument.Meter.Name == Telemetry.Name && instrument.Name == "heartbeat_sources_active")
      {
        listener.EnableMeasurementEvents(instrument);
      }
    };
    meterListener.SetMeasurementEventCallback<int>((_, measurement, _, _) => measurements.Add(measurement));
    meterListener.Start();
    meterListener.RecordObservableInstruments();

    await Assert.That(measurements).Contains(3);
  }

  private static TimeProvider Clock()
  {
    var clock = Substitute.For<TimeProvider>();
    clock.TimestampFrequency.Returns(TimeSpan.TicksPerSecond);
    clock.GetTimestamp().Returns(0L);
    clock.GetUtcNow().Returns(Start);
    return clock;
  }

  private static HeartbeatWatchdog Watchdog(HeartbeatRegistry registry, TimeProvider timeProvider)
  {
    var options = Substitute.For<IOptionsMonitor<HeartbeatOptions>>();
    options.CurrentValue.Returns(new HeartbeatOptions());
    return new HeartbeatWatchdog(registry, timeProvider, options, NullLogger<HeartbeatWatchdog>.Instance);
  }
}
