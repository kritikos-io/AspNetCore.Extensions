namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using Kritikos.AspNetCore.MinimalApiExtensions.Options;
using Kritikos.AspNetCore.MinimalApiExtensions.Services.Heartbeat;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using NSubstitute;

public sealed class HeartbeatHealthCheckTests
{
  private static readonly DateTimeOffset Start = DateTimeOffset.UnixEpoch;

  [Test]
  public async Task Healthy_when_no_sources_registered()
  {
    var (_, check, context) = Arrange(Clock());

    var result = await check.CheckHealthAsync(context);

    await Assert.That(result.Status).IsEqualTo(HealthStatus.Healthy);
  }

  [Test]
  public async Task Healthy_when_all_sources_are_fresh()
  {
    var (registry, check, context) = Arrange(Clock());
    registry.Register("walker", TimeSpan.FromSeconds(30));

    var result = await check.CheckHealthAsync(context);

    await Assert.That(result.Status).IsEqualTo(HealthStatus.Healthy);
  }

  [Test]
  public async Task Unhealthy_when_a_source_is_stale()
  {
    var clock = Clock();
    var (registry, check, context) = Arrange(clock);
    registry.Register("walker", TimeSpan.FromSeconds(5));

    clock.GetTimestamp().Returns(TimeSpan.FromSeconds(6).Ticks);
    var result = await check.CheckHealthAsync(context);

    await Assert.That(result.Status).IsEqualTo(HealthStatus.Unhealthy);
    await Assert.That(result.Data.ContainsKey("walker")).IsTrue();
  }

  [Test]
  public async Task Unhealthy_when_a_cancelled_source_keeps_beating()
  {
    var clock = Clock();
    var (registry, check, context) = Arrange(clock);
    var handle = registry.Register("walker", TimeSpan.FromSeconds(5));

    clock.GetTimestamp().Returns(TimeSpan.FromSeconds(6).Ticks);
    registry.CancelStaleSources(TimeSpan.FromSeconds(15));
    handle.Beat(); // a bitten source cannot beat its way back to healthy

    var result = await check.CheckHealthAsync(context);

    await Assert.That(result.Status).IsEqualTo(HealthStatus.Unhealthy);
  }

  [Test]
  public async Task Uses_default_timeout_when_source_has_no_timeout()
  {
    var clock = Clock();
    var (registry, check, context) = Arrange(clock);
    registry.Register("walker");

    clock.GetTimestamp().Returns(TimeSpan.FromSeconds(20).Ticks);
    var result = await check.CheckHealthAsync(context);

    await Assert.That(result.Status).IsEqualTo(HealthStatus.Unhealthy);
  }

  [Test]
  public async Task Reports_the_registrations_failure_status()
  {
    var clock = Clock();
    var (registry, check, context) = Arrange(clock, HealthStatus.Degraded);
    registry.Register("walker", TimeSpan.FromSeconds(5));

    clock.GetTimestamp().Returns(TimeSpan.FromSeconds(6).Ticks);
    var result = await check.CheckHealthAsync(context);

    await Assert.That(result.Status).IsEqualTo(HealthStatus.Degraded);
  }

  private static TimeProvider Clock()
  {
    var clock = Substitute.For<TimeProvider>();
    clock.TimestampFrequency.Returns(TimeSpan.TicksPerSecond);
    clock.GetTimestamp().Returns(0L);
    clock.GetUtcNow().Returns(Start);
    return clock;
  }

  private static (HeartbeatRegistry Registry, HeartbeatHealthCheck Check, HealthCheckContext Context) Arrange(
    TimeProvider timeProvider,
    HealthStatus failureStatus = HealthStatus.Unhealthy)
  {
    var options = Substitute.For<IOptionsMonitor<HeartbeatOptions>>();
    options.CurrentValue.Returns(new HeartbeatOptions());
    var registry = new HeartbeatRegistry(timeProvider);
    var check = new HeartbeatHealthCheck(registry, options);
    var context = new HealthCheckContext
    {
      Registration = new HealthCheckRegistration(HeartbeatHealthCheck.Name, check, failureStatus, tags: null),
    };
    return (Registry: registry, Check: check, Context: context);
  }
}
