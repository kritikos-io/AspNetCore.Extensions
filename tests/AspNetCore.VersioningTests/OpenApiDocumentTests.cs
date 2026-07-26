#pragma warning disable CA2234 // Pass system uri objects instead of strings
namespace Kritikos.AspNetCore.VersioningTests;

using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

[ClassDataSource<WebApplicationFactory<Program>>(Shared = SharedType.PerClass)]
public class OpenApiDocumentTests(WebApplicationFactory<Program> factory)
{
  private const string DiscoveryJson =
    """
    {
      "issuer": "https://auth.test",
      "authorization_endpoint": "https://auth.test/connect/authorize",
      "token_endpoint": "https://auth.test/connect/token"
    }
    """;

  [Test]
  public async Task Version_document_includes_info_and_oidc_security_scheme(CancellationToken cancellationToken)
  {
    var client = CreateClient();

    var response = await client.GetAsync("/openapi/v1.json", cancellationToken);
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
    var root = json.RootElement;

    await Assert.That(root.GetProperty("info").GetProperty("title").GetString()).IsEqualTo("PetStore API");

    await Assert.That(root.TryGetProperty("components", out var components)).IsTrue();
    await Assert.That(components.TryGetProperty("securitySchemes", out var schemes)).IsTrue();
    await Assert.That(schemes.TryGetProperty("openid", out var openid)).IsTrue();
    await Assert.That(openid.GetProperty("type").GetString()).IsEqualTo("oauth2");
  }

  [Test]
  public async Task Each_api_version_is_served_as_its_own_document(CancellationToken cancellationToken)
  {
    var client = CreateClient();

    var v1 = await client.GetAsync("/openapi/v1.json", cancellationToken);
    var v2 = await client.GetAsync("/openapi/v2.json", cancellationToken);

    await Assert.That(v1.StatusCode).IsEqualTo(HttpStatusCode.OK);
    await Assert.That(v2.StatusCode).IsEqualTo(HttpStatusCode.OK);
  }

  private HttpClient CreateClient()
    => factory
      .WithWebHostBuilder(static builder =>
        builder.ConfigureTestServices(static services =>
          services.ConfigureHttpClientDefaults(static http =>
            http.ConfigurePrimaryHttpMessageHandler(static () => new DiscoveryStubHandler(DiscoveryJson)))))
      .CreateClient();

  private sealed class DiscoveryStubHandler(string discovery) : HttpMessageHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken cancellationToken)
      => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(discovery, Encoding.UTF8, "application/json"),
      });
  }
}
