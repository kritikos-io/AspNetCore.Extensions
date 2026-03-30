#pragma warning disable CA2234 // Pass system uri objects instead of strings

namespace Kritikos.AspNetCore.FeatureManagementOptionTests;

using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

[ClassDataSource<WebApplicationFactory<Program>>(Shared = SharedType.PerClass)]
public class FeatureGateTests(WebApplicationFactory<Program> factory)
{
  [Test]
  public async Task EndpointFeatureFilter_should_be_transparent_when_feature_is_enabled(CancellationToken cancellationToken)
  {
    var client = factory.WithWebHostBuilder(static builder => builder
            .UseSetting("FeatureManagement:MyFeature", "true"))
        .CreateClient();

    var response = await client.GetAsync("/api/v2/feature/single", cancellationToken);
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

    var fromJsonAsync = await response.Content.ReadFromJsonAsync<string>(cancellationToken);
    await Assert.That(fromJsonAsync).IsEqualTo("on");
  }

  [Test]
  public async Task EndpointFeatureFilter_should_return_not_found_when_feature_is_disabled(CancellationToken cancellationToken)
  {
    var client = factory.WithWebHostBuilder(static builder => builder
            .UseSetting("FeatureManagement:MyFeature", "false"))
        .CreateClient();

    var response = await client.GetAsync("/api/v2/feature/single", cancellationToken);
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
  }
}
