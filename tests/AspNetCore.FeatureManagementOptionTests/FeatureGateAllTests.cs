#pragma warning disable CA2234 // Pass system uri objects instead of strings

namespace Kritikos.AspNetCore.FeatureManagementOptionTests;

using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

public class FeatureGateAllTests
  : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> factory;

  public FeatureGateAllTests(WebApplicationFactory<Program> factory)
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
  public async Task EndpointFeatureFilter_with_and_filter_should_return_not_found_when_both_features_are_disabled()
  {
    var client = factory.WithWebHostBuilder(builder =>
      {
        builder.UseSetting("FeatureManagement:FirstAndFlag", "false");
        builder.UseSetting("FeatureManagement:SecondAndFlag", "false");
      })
      .CreateClient();

    var response = await client.GetAsync("/api/feature/and");
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task EndpointFeatureFilter_with_and_filter_should_should_return_not_found_when_only_first_flag_is_enabled()
  {
    var client = factory.WithWebHostBuilder(builder =>
      {
        builder.UseSetting("FeatureManagement:FirstAndFlag", "true");
        builder.UseSetting("FeatureManagement:SecondAndFlag", "false");
      })
      .CreateClient();

    var response = await client.GetAsync("/api/feature/and");
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task EndpointFeatureFilter_with_and_filter_should_should_return_not_found_when_only_second_flag_is_enabled()
  {
    var client = factory.WithWebHostBuilder(builder =>
      {
        builder.UseSetting("FeatureManagement:FirstAndFlag", "false");
        builder.UseSetting("FeatureManagement:SecondAndFlag", "true");
      })
      .CreateClient();

    var response = await client.GetAsync("/api/feature/and");
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task EndpointFeatureFilter_with_and_filter_should_be_transparent_when_both_flags_are_enabled()
  {
    var client = factory.WithWebHostBuilder(builder =>
      {
        builder.UseSetting("FeatureManagement:FirstAndFlag", "true");
        builder.UseSetting("FeatureManagement:SecondAndFlag", "true");
      })
      .CreateClient();

    var response = await client.GetAsync("/api/feature/and");
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var fromJsonAsync = await response.Content.ReadFromJsonAsync<string>();
    fromJsonAsync.Should().Be("on");
  }
}
