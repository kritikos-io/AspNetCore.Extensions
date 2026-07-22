namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using Kritikos.AspNetCore.MinimalApiExtensions.Services.Heartbeat;

public sealed class HeartbeatConcurrencyTests
{
  [Test]
  public async Task Concurrent_reregistration_of_a_name_leaves_one_live_source()
  {
    var registry = new HeartbeatRegistry(TimeProvider.System);

    Parallel.For(0, 500, _ => registry.Register("walker"));

    // Every registration replaces (and disposes) the prior handle, so exactly one survives.
    await Assert.That(registry.Snapshot().Count).IsEqualTo(1);
  }

  [Test]
  public async Task Concurrent_beat_scan_and_reregister_never_throw_or_deadlock()
  {
    var registry = new HeartbeatRegistry(TimeProvider.System);
    var handle = registry.Register("walker", TimeSpan.FromMilliseconds(1));
    using var stop = new CancellationTokenSource(TimeSpan.FromMilliseconds(750));
    var token = stop.Token;

    var workers = new[]
    {
      Task.Run(() =>
      {
        while (!token.IsCancellationRequested)
        {
          handle.Beat();
        }
      }),
      Task.Run(() =>
      {
        while (!token.IsCancellationRequested)
        {
          registry.CancelStaleSources(TimeSpan.FromMilliseconds(1));
        }
      }),
      Task.Run(() =>
      {
        while (!token.IsCancellationRequested)
        {
          registry.Register("walker", TimeSpan.FromMilliseconds(1)).Dispose();
        }
      }),
    };

    // A deadlock would blow the WaitAsync budget; an unhandled race would surface as a faulted task.
    await Task.WhenAll(workers).WaitAsync(TimeSpan.FromSeconds(10));

    await Assert.That(token.IsCancellationRequested).IsTrue();
  }
}
