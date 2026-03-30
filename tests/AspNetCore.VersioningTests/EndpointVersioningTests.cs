#pragma warning disable CA2234 // Pass system uri objects instead of strings
namespace Kritikos.AspNetCore.VersioningTests;

using System.Net;
using System.Net.Http.Json;

using Kritikos.PetStore.WebApi.Models.V1;
using Kritikos.PetStore.WebApi.Models.V2;

using Microsoft.AspNetCore.Mvc.Testing;

[ClassDataSource<WebApplicationFactory<Program>>(Shared = SharedType.PerClass)]
public class EndpointVersioningTests(WebApplicationFactory<Program> factory)
{
  [Test]
  public async Task Ensure_V1_Endpoint_Returns_Proper_Type(CancellationToken cancellationToken)
  {
    var client = factory.WithWebHostBuilder(static _ => { })
        .CreateClient();

    var response = await client.GetAsync("/api/v1/pet/5", cancellationToken);
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

    var fromJsonAsync = await response.Content.ReadFromJsonAsync<PetV1Dto>(cancellationToken);
    await Assert.That(fromJsonAsync).IsNotNull();

    var expected = new PetV1Dto("Sir Paddington", 3);
    await Assert.That(fromJsonAsync).IsEqualTo(expected);
  }

  [Test]
  public async Task Ensure_V2_Endpoint_Returns_Proper_Type(CancellationToken cancellationToken)
  {
    var client = factory.WithWebHostBuilder(static _ => { })
        .CreateClient();

    var response = await client.GetAsync("/api/v2/pet/5", cancellationToken);
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

    var fromJsonAsync = await response.Content.ReadFromJsonAsync<PetV2Dto>(cancellationToken);
    await Assert.That(fromJsonAsync).IsNotNull();

    var expected = new PetV2Dto("Snuggles", "McFluff", 5);
    await Assert.That(fromJsonAsync).IsEqualTo(expected);
  }
}
