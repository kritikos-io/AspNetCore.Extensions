namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using Kritikos.AspNetCore.MinimalApiExtensions.Services.Heartbeat;

using NSubstitute;

public sealed class HeartbeatRegistryTests
{
  private static readonly DateTimeOffset Start = DateTimeOffset.UnixEpoch;

  [Test]
  public async Task Register_seeds_beat_and_stores_timeout()
  {
    var registry = new HeartbeatRegistry(Clock());

    registry.Register("walker", TimeSpan.FromSeconds(30));

    var snapshot = registry.Snapshot();
    await Assert.That(snapshot.Count).IsEqualTo(1);
    await Assert.That(snapshot[0].Name).IsEqualTo("walker");
    await Assert.That(snapshot[0].LastBeat).IsEqualTo(Start);
    await Assert.That(snapshot[0].Timeout).IsEqualTo(TimeSpan.FromSeconds(30));
  }

  [Test]
  public async Task Beat_refreshes_the_source_timestamp()
  {
    var clock = Clock();
    var registry = new HeartbeatRegistry(clock);
    var heartbeat = registry.Register("walker");

    var later = Start + TimeSpan.FromSeconds(10);
    clock.GetUtcNow().Returns(later);
    heartbeat.Beat();

    var snapshot = registry.Snapshot();
    await Assert.That(snapshot[0].LastBeat).IsEqualTo(later);
  }

  [Test]
  public async Task Beat_does_not_refresh_a_cancelled_source()
  {
    var clock = Clock();
    var registry = new HeartbeatRegistry(clock);
    var heartbeat = registry.Register("walker", TimeSpan.FromSeconds(5));

    clock.GetTimestamp().Returns(TimeSpan.FromSeconds(6).Ticks);
    registry.CancelStaleSources(TimeSpan.FromSeconds(15));
    heartbeat.Beat(); // a bitten source must not beat its way back to fresh

    var snapshot = registry.Snapshot();
    await Assert.That(snapshot[0].IsCancelled).IsTrue();
    await Assert.That(snapshot[0].Age).IsGreaterThanOrEqualTo(TimeSpan.FromSeconds(6));
  }

  [Test]
  public async Task Dispose_unregisters_the_source()
  {
    var registry = new HeartbeatRegistry(Clock());
    var heartbeat = registry.Register("walker");

    heartbeat.Dispose();

    await Assert.That(registry.Snapshot().Count).IsEqualTo(0);
  }

  [Test]
  public async Task Register_disposes_the_previous_handle_on_replace()
  {
    var registry = new HeartbeatRegistry(Clock());
    var first = registry.Register("walker");

    registry.Register("walker"); // replaces and disposes the previous handle

    await Assert.That(() => first.Token).Throws<ObjectDisposedException>();
  }

  [Test]
  public async Task Disposing_a_stale_handle_after_reregistration_keeps_the_new_source()
  {
    var registry = new HeartbeatRegistry(Clock());
    var first = registry.Register("walker");
    registry.Register("walker"); // replaces "walker" with a new handle

    first.Dispose(); // stale handle must not remove the live registration

    var snapshot = registry.Snapshot();
    await Assert.That(snapshot.Count).IsEqualTo(1);
    await Assert.That(snapshot[0].Name).IsEqualTo("walker");
  }

  [Test]
  public async Task CancelStaleSources_cancels_a_stale_source()
  {
    var clock = Clock();
    var registry = new HeartbeatRegistry(clock);
    var heartbeat = registry.Register("walker", TimeSpan.FromSeconds(5));

    clock.GetTimestamp().Returns(TimeSpan.FromSeconds(6).Ticks);
    var outcome = registry.CancelStaleSources(TimeSpan.FromSeconds(15));

    await Assert.That(outcome.Cancelled.Contains("walker")).IsTrue();
    await Assert.That(outcome.Failures.Count).IsEqualTo(0);
    await Assert.That(heartbeat.Token.IsCancellationRequested).IsTrue();
  }

  [Test]
  public async Task CancelStaleSources_leaves_fresh_sources_untouched()
  {
    var clock = Clock();
    var registry = new HeartbeatRegistry(clock);
    var heartbeat = registry.Register("walker", TimeSpan.FromSeconds(30));

    clock.GetTimestamp().Returns(TimeSpan.FromSeconds(5).Ticks);
    var outcome = registry.CancelStaleSources(TimeSpan.FromSeconds(15));

    await Assert.That(outcome.Cancelled.Count).IsEqualTo(0);
    await Assert.That(heartbeat.Token.IsCancellationRequested).IsFalse();
  }

  [Test]
  public async Task CancelStaleSources_reports_a_failure_when_a_cancellation_callback_throws()
  {
    var clock = Clock();
    var registry = new HeartbeatRegistry(clock);
    var faulting = registry.Register("faulting", TimeSpan.FromSeconds(5));
    var runner = registry.Register("runner", TimeSpan.FromSeconds(5));
    faulting.Token.Register(static () => throw new InvalidOperationException("callback boom"));

    clock.GetTimestamp().Returns(TimeSpan.FromSeconds(6).Ticks);
    var outcome = registry.CancelStaleSources(TimeSpan.FromSeconds(15));

    await Assert.That(outcome.Failures.Any(failure => failure.Source == "faulting")).IsTrue();
    await Assert.That(outcome.Cancelled.Contains("runner")).IsTrue();
    await Assert.That(faulting.Token.IsCancellationRequested).IsTrue();
    await Assert.That(runner.Token.IsCancellationRequested).IsTrue();
  }

  private static TimeProvider Clock()
  {
    var clock = Substitute.For<TimeProvider>();
    clock.TimestampFrequency.Returns(TimeSpan.TicksPerSecond);
    clock.GetTimestamp().Returns(0L);
    clock.GetUtcNow().Returns(Start);
    return clock;
  }
}
