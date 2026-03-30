#pragma warning disable CA2234 // Pass system uri objects instead of strings

namespace Kritikos.AspNetCore.MinimalApiTests;

using Kritikos.AspNetCore.MinimalApiExtensions.Options;

using Microsoft.AspNetCore.Mvc.Testing;

[ClassDataSource<WebApplicationFactory<Program>>(Shared = SharedType.PerClass)]
public class CorrelationMiddlewareTests(WebApplicationFactory<Program> factory)
{
  [Test]
  public async Task When_request_has_no_header_random_value_is_generated(CancellationToken cancellationToken)
  {
    var client = factory.CreateClient();

    var response = await client.GetAsync("api/v1/pet/3", cancellationToken);

    response.EnsureSuccessStatusCode();
    await Assert.That(response.Headers.Contains(CorrelationHeaderOptions.DefaultHeader)).IsTrue();
  }

  [Test]
  public async Task When_request_has_header_response_should_contain_same_value(CancellationToken cancellationToken)
  {
    const string correlation = "a-not-so-random-string";
    var client = factory.CreateClient();

    client.DefaultRequestHeaders.Add(CorrelationHeaderOptions.DefaultHeader, correlation);
    var response = await client.GetAsync("api/v1/pet/3", cancellationToken);

    response.EnsureSuccessStatusCode();
    await Assert.That(response.Headers.Contains(CorrelationHeaderOptions.DefaultHeader)).IsTrue();
    await Assert.That(response.Headers.GetValues(CorrelationHeaderOptions.DefaultHeader).First()).IsEqualTo(correlation);
  }
}
