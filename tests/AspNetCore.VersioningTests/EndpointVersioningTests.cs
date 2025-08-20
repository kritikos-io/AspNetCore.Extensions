#pragma warning disable CA2234 // Pass system uri objects instead of strings
namespace Kritikos.AspNetCore.VersioningTests;

using System.Net;
using System.Net.Http.Json;

using Kritikos.AspNetCore.MinimalApiExtensions.Extensions;
using Kritikos.AspNetCore.VersioningOptions;
using Kritikos.PetStore.WebApi.Models.V1;
using Kritikos.PetStore.WebApi.Models.V2;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

public class EndpointVersioningTests
    : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> factory;

  public EndpointVersioningTests(WebApplicationFactory<Program> factory)
  {
    ArgumentNullException.ThrowIfNull(factory);

    this.factory = factory;
    factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureTestServices(services =>
      {
        services.AddApiVersioningDefaults();
        services.AddEndpoints(this.GetType().Assembly);
      });
    });
  }

  [Fact]
  public async Task Ensure_V1_Endpoint_Returns_Proper_Type()
  {
    var client = factory.WithWebHostBuilder(static _ => { })
        .CreateClient();

    var response = await client.GetAsync("/api/v1/pet/5", TestContext.Current.CancellationToken);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var fromJsonAsync = await response.Content.ReadFromJsonAsync<PetV1Dto>(TestContext.Current.CancellationToken);
    Assert.NotNull(fromJsonAsync);

    var expected = new PetV1Dto("Sir Paddington", 3);
    Assert.Equal(expected, fromJsonAsync);
    Assert.Equal(expected, fromJsonAsync);
  }

  [Fact]
  public async Task Ensure_V2_Endpoint_Returns_Proper_Type()
  {
    var client = factory.WithWebHostBuilder(static _ => { })
        .CreateClient();

    var response = await client.GetAsync("/api/v2/pet/5", TestContext.Current.CancellationToken);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var fromJsonAsync = await response.Content.ReadFromJsonAsync<PetV2Dto>(TestContext.Current.CancellationToken);
    Assert.NotNull(fromJsonAsync);

    var expected = new PetV2Dto("Snuggles", "McFluff", 5);
    Assert.Equal(expected, fromJsonAsync);
  }
}
