#pragma warning disable CA2234 // Pass system uri objects instead of strings

namespace Kritikos.AspNetCore.FeatureManagementOptionTests;

using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

public class FeatureGateTests
  : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> factory;

  public FeatureGateTests(WebApplicationFactory<Program> factory)
  {
    ArgumentNullException.ThrowIfNull(factory);

    this.factory = factory;

    factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services =>
      {
        services.AddFeatureManagement();
        services.AddEndpoints(this.GetType().Assembly);
      });
    });
  }

  [Fact]
  public async Task EndpointFeatureFilter_should_be_transparent_when_feature_is_enabled()
  {
    var client = factory.WithWebHostBuilder(builder =>
      {
        builder.UseSetting("FeatureManagement:MyFeature", "true");
      })
      .CreateClient();

    var response = await client.GetAsync("/api/feature/single");
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var fromJsonAsync = await response.Content.ReadFromJsonAsync<string>();
    fromJsonAsync.Should().Be("on");
  }

  [Fact]
  public async Task EndpointFeatureFilter_should_return_not_found_when_feature_is_disabled()
  {
    var client = factory.WithWebHostBuilder(builder =>
      {
        builder.UseSetting("FeatureManagement:MyFeature", "false");
      })
      .CreateClient();

    var response = await client.GetAsync("/api/feature/single");
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }
}
