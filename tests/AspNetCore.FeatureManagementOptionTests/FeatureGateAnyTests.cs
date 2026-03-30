#pragma warning disable CA2234 // Pass system uri objects instead of strings

namespace Kritikos.AspNetCore.FeatureManagementOptionTests;

using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

[ClassDataSource<WebApplicationFactory<Program>>(Shared = SharedType.PerClass)]
public class FeatureGateAnyTests(WebApplicationFactory<Program> factory)
{
  [Test]
  public async Task EndpointFeatureFilter_with_or_filter_should_return_not_found_when_both_features_are_disabled(CancellationToken cancellationToken)
  {
    var client = factory.WithWebHostBuilder(static builder =>
        {
          builder.UseSetting("FeatureManagement:FirstOrFlag", "false");
          builder.UseSetting("FeatureManagement:SecondOrFlag", "false");
        })
        .CreateClient();

    var response = await client.GetAsync("/api/v2/feature/or", cancellationToken);
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
  }

  [Test]
  public async Task EndpointFeatureFilter_with_and_filter_should_be_transparent_when_only_first_flag_is_enabled(CancellationToken cancellationToken)
  {
    var client = factory.WithWebHostBuilder(static builder =>
        {
          builder.UseSetting("FeatureManagement:FirstOrFlag", "true");
          builder.UseSetting("FeatureManagement:SecondOrFlag", "false");
        })
        .CreateClient();

    var response = await client.GetAsync("/api/v2/feature/or", cancellationToken);
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

    var fromJsonAsync = await response.Content.ReadFromJsonAsync<string>(cancellationToken);
    await Assert.That(fromJsonAsync).IsEqualTo("on");
  }

  [Test]
  public async Task EndpointFeatureFilter_with_or_filter_should_be_transparent_when_only_second_flag_is_enabled(CancellationToken cancellationToken)
  {
    var client = factory.WithWebHostBuilder(static builder =>
        {
          builder.UseSetting("FeatureManagement:FirstOrFlag", "false");
          builder.UseSetting("FeatureManagement:SecondOrFlag", "true");
        })
        .CreateClient();

    var response = await client.GetAsync("/api/v2/feature/or", cancellationToken);
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

    var fromJsonAsync = await response.Content.ReadFromJsonAsync<string>(cancellationToken);
    await Assert.That(fromJsonAsync).IsEqualTo("on");
  }

  [Test]
  public async Task EndpointFeatureFilter_with_or_filter_should_be_transparent_when_both_flags_are_enabled(CancellationToken cancellationToken)
  {
    var client = factory.WithWebHostBuilder(static builder =>
        {
          builder.UseSetting("FeatureManagement:FirstOrFlag", "true");
          builder.UseSetting("FeatureManagement:SecondOrFlag", "true");
        })
        .CreateClient();

    var response = await client.GetAsync("/api/v2/feature/or", cancellationToken);
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

    var fromJsonAsync = await response.Content.ReadFromJsonAsync<string>(cancellationToken);
    await Assert.That(fromJsonAsync).IsEqualTo("on");
  }
}
