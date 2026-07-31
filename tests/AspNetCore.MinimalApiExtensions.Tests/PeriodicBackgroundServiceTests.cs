namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using Kritikos.AspNetCore.MinimalApiExtensions.Options;
using Kritikos.AspNetCore.MinimalApiExtensions.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NSubstitute;

public sealed class PeriodicBackgroundServiceTests
{
  [Test]
  public async Task Constructor_builds_the_timer_from_the_injected_time_provider()
  {
    var timeProvider = Substitute.For<TimeProvider>();
    TimeSpan? period = null;
    timeProvider
      .CreateTimer(Arg.Any<TimerCallback>(), Arg.Any<object?>(), Arg.Any<TimeSpan>(), Arg.Any<TimeSpan>())
      .Returns(call =>
      {
        period = call.ArgAt<TimeSpan>(3);
        return Substitute.For<ITimer>();
      });
    var options = Options.Create(new TestOptions { Interval = TimeSpan.FromMinutes(5) });

    using var service = new TestService(options, timeProvider);

    await Assert.That(period).IsEqualTo(TimeSpan.FromMinutes(5));
  }

  [Test]
  public async Task ExecuteAsync_runs_the_work_when_the_injected_timer_ticks(CancellationToken ct)
  {
    var timeProvider = Substitute.For<TimeProvider>();
    TimerCallback? tick = null;
    object? tickState = null;
    timeProvider
      .CreateTimer(Arg.Any<TimerCallback>(), Arg.Any<object?>(), Arg.Any<TimeSpan>(), Arg.Any<TimeSpan>())
      .Returns(call =>
      {
        tick = call.Arg<TimerCallback>();
        tickState = call.ArgAt<object?>(1);
        return Substitute.For<ITimer>();
      });
    var worked = new TaskCompletionSource();
    var options = Options.Create(new TestOptions { Interval = TimeSpan.FromMinutes(5) });
    using var service = new TestService(options, timeProvider, () => worked.TrySetResult());

    await service.StartAsync(ct);
    var ranBeforeTick = worked.Task.IsCompleted;
    tick!.Invoke(tickState);
    await worked.Task.WaitAsync(TimeSpan.FromSeconds(10), ct);
    await service.StopAsync(ct);

    await Assert.That(ranBeforeTick).IsFalse();
    await Assert.That(worked.Task.IsCompletedSuccessfully).IsTrue();
  }

  private sealed class TestOptions : PeriodicBackgroundServiceOptions;

  private sealed class TestService(IOptions<TestOptions> options, TimeProvider timeProvider, Action? onWork = null)
    : PeriodicBackgroundService<TestService, TestOptions>(options, NullLogger<TestService>.Instance, timeProvider)
  {
    protected override Task DoWork(CancellationToken stoppingToken)
    {
      onWork?.Invoke();
      return Task.CompletedTask;
    }
  }
}
