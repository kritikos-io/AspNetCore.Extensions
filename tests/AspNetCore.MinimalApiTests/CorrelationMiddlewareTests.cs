#pragma warning disable CA2234 // Pass system uri objects instead of strings

namespace Kritikos.AspNetCore.MinimalApiTests;

using Kritikos.AspNetCore.MinimalApiExtensions.Options;

using Microsoft.AspNetCore.Mvc.Testing;

public class CorrelationMiddlewareTests
    : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> factory;

  public CorrelationMiddlewareTests(WebApplicationFactory<Program> factory)
  {
    ArgumentNullException.ThrowIfNull(factory);
    this.factory = factory;

    factory.WithWebHostBuilder(static _ => { });
  }

  [Fact]
  public async Task When_request_has_no_header_random_value_is_generated()
  {
    var client = factory.CreateClient();

    var response = await client.GetAsync("api/v1/pet/3");

    response.EnsureSuccessStatusCode();
    Assert.True(response.Headers.Contains(CorrelationHeaderOptions.DefaultHeader));
  }

  [Fact]
  public async Task When_request_has_header_response_should_contain_same_value()
  {
    const string correlation = "a-not-so-random-string";
    var client = factory.CreateClient();

    client.DefaultRequestHeaders.Add(CorrelationHeaderOptions.DefaultHeader, correlation);
    var response = await client.GetAsync("api/v1/pet/3");

    response.EnsureSuccessStatusCode();
    Assert.True(response.Headers.Contains(CorrelationHeaderOptions.DefaultHeader));
    Assert.Equal(correlation, response.Headers.GetValues(CorrelationHeaderOptions.DefaultHeader).First());
  }
}
