namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using Kritikos.AspNetCore.MinimalApiExtensions.Options;
using Kritikos.AspNetCore.MinimalApiExtensions.Services.Heartbeat;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NSubstitute;

public sealed class HeartbeatWatchdogTests
{
  private static readonly DateTimeOffset Start = DateTimeOffset.UnixEpoch;

  [Test]
  public async Task Scan_cancels_a_source_once_its_beat_goes_stale()
  {
    var clock = Clock();
    var registry = new HeartbeatRegistry(clock);
    var handle = registry.Register("walker", TimeSpan.FromSeconds(5));
    var watchdog = Watchdog(registry, clock);

    clock.GetTimestamp().Returns(TimeSpan.FromSeconds(6).Ticks);
    watchdog.Scan();

    await Assert.That(handle.Token.IsCancellationRequested).IsTrue();
  }

  [Test]
  public async Task Scan_leaves_a_fresh_source_running()
  {
    var clock = Clock();
    var registry = new HeartbeatRegistry(clock);
    var handle = registry.Register("walker", TimeSpan.FromSeconds(30));
    var watchdog = Watchdog(registry, clock);

    clock.GetTimestamp().Returns(TimeSpan.FromSeconds(10).Ticks);
    watchdog.Scan();

    await Assert.That(handle.Token.IsCancellationRequested).IsFalse();
  }

  [Test]
  public async Task Scan_uses_default_timeout_when_source_has_no_timeout()
  {
    var clock = Clock();
    var registry = new HeartbeatRegistry(clock);
    var handle = registry.Register("walker");
    var watchdog = Watchdog(registry, clock);

    clock.GetTimestamp().Returns(TimeSpan.FromSeconds(20).Ticks);
    watchdog.Scan();

    await Assert.That(handle.Token.IsCancellationRequested).IsTrue();
  }

  [Test]
  public async Task Scan_survives_a_throwing_cancellation_callback()
  {
    var clock = Clock();
    var registry = new HeartbeatRegistry(clock);
    var handle = registry.Register("walker", TimeSpan.FromSeconds(5));
    handle.Token.Register(static () => throw new InvalidOperationException("callback boom"));
    var watchdog = Watchdog(registry, clock);

    clock.GetTimestamp().Returns(TimeSpan.FromSeconds(6).Ticks);
    watchdog.Scan();

    await Assert.That(handle.Token.IsCancellationRequested).IsTrue();
  }

  [Test]
  public async Task ExecuteAsync_applies_a_shortened_interval_without_a_restart()
  {
    var registry = new HeartbeatRegistry(TimeProvider.System);
    var options = new TestOptionsMonitor<HeartbeatOptions>(new HeartbeatOptions
    {
      WatchdogInterval = TimeSpan.FromSeconds(30),
      DefaultTimeout = TimeSpan.FromMilliseconds(50),
    });
    using var watchdog = new HeartbeatWatchdog(
      registry,
      TimeProvider.System,
      options,
      NullLogger<HeartbeatWatchdog>.Instance);
    var handle = registry.Register("walker");

    await watchdog.StartAsync(CancellationToken.None);
    var cancelledUnderLongInterval =
      await WaitUntilAsync(() => handle.Token.IsCancellationRequested, TimeSpan.FromMilliseconds(250));

    options.Set(new HeartbeatOptions
    {
      WatchdogInterval = TimeSpan.FromMilliseconds(50),
      DefaultTimeout = TimeSpan.FromMilliseconds(50),
    });
    var cancelledAfterReload =
      await WaitUntilAsync(() => handle.Token.IsCancellationRequested, TimeSpan.FromSeconds(2));

    await watchdog.StopAsync(CancellationToken.None);

    await Assert.That(cancelledUnderLongInterval).IsFalse();
    await Assert.That(cancelledAfterReload).IsTrue();
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

  private static async Task<bool> WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
  {
    var deadline = DateTimeOffset.UtcNow + timeout;
    while (DateTimeOffset.UtcNow < deadline)
    {
      if (condition())
      {
        return true;
      }

      await Task.Delay(TimeSpan.FromMilliseconds(20));
    }

    return condition();
  }

  private sealed class TestOptionsMonitor<T>(T initial) : IOptionsMonitor<T>
    where T : class
  {
    private readonly List<Action<T, string?>> listeners = [];

    public T CurrentValue { get; private set; } = initial;

    public T Get(string? name) => CurrentValue;

    public IDisposable OnChange(Action<T, string?> listener)
    {
      listeners.Add(listener);
      return new Subscription(listeners, listener);
    }

    public void Set(T value)
    {
      CurrentValue = value;
      foreach (var listener in listeners.ToArray())
      {
        listener(value, null);
      }
    }

    private sealed class Subscription(List<Action<T, string?>> listeners, Action<T, string?> listener) : IDisposable
    {
      public void Dispose() => listeners.Remove(listener);
    }
  }
}
