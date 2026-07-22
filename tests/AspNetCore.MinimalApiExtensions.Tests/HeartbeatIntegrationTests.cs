#pragma warning disable CA2234 // Pass System.Uri objects instead of strings

namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using System.Net;

using Kritikos.AspNetCore.MinimalApiExtensions.Extensions;
using Kritikos.AspNetCore.MinimalApiExtensions.Services.Heartbeat;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.TestHost;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public sealed class HeartbeatIntegrationTests
{
  [Test]
  public async Task Live_endpoint_is_healthy_when_no_sources_are_registered(CancellationToken cancellationToken)
  {
    await using var app = await StartHostAsync();
    using var client = app.GetTestClient();

    var response = await client.GetAsync("/health/live", cancellationToken);

    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
  }

  [Test]
  public async Task Live_endpoint_turns_unhealthy_once_a_source_goes_stale(CancellationToken cancellationToken)
  {
    await using var app = await StartHostAsync();
    using var client = app.GetTestClient();
    var registry = app.Services.GetRequiredService<HeartbeatRegistry>();

    using var heartbeat = registry.Register("import", TimeSpan.FromMilliseconds(50));
    await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);

    var response = await client.GetAsync("/health/live", cancellationToken);

    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.ServiceUnavailable);
  }

  private static async Task<WebApplication> StartHostAsync()
  {
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();
    builder.Logging.ClearProviders();
    builder.Services.AddHeartbeat();

    var app = builder.Build();
    app.MapHealthChecks(
      "/health/live",
      new HealthCheckOptions { Predicate = registration => registration.Tags.Contains("live") });

    await app.StartAsync();
    return app;
  }
}
