#pragma warning disable CA2234 // Pass system uri objects instead of strings
namespace Kritikos.AspNetCore.VersioningTests;

using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

[ClassDataSource<WebApplicationFactory<Program>>(Shared = SharedType.PerClass)]
public class OpenApiDocumentTests(WebApplicationFactory<Program> factory)
{
  [Test]
  public async Task Version_document_includes_info_and_oauth2_security_scheme(CancellationToken cancellationToken)
  {
    var client = factory.CreateClient();

    var response = await client.GetAsync("/openapi/v1.json", cancellationToken);
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
    var root = json.RootElement;

    await Assert.That(root.GetProperty("info").GetProperty("title").GetString()).IsEqualTo("PetStore API");

    await Assert.That(root.TryGetProperty("components", out var components)).IsTrue();
    await Assert.That(components.TryGetProperty("securitySchemes", out var schemes)).IsTrue();
    await Assert.That(schemes.TryGetProperty("oauth2", out var oauth2)).IsTrue();
    await Assert.That(oauth2.GetProperty("type").GetString()).IsEqualTo("oauth2");

    var authorizationCode = oauth2.GetProperty("flows").GetProperty("authorizationCode");
    await Assert.That(authorizationCode.GetProperty("authorizationUrl").GetString())
      .Contains("/protocol/openid-connect/auth");
    await Assert.That(authorizationCode.GetProperty("tokenUrl").GetString())
      .Contains("/protocol/openid-connect/token");
  }

  [Test]
  public async Task Secured_operation_requires_the_oauth2_scheme(CancellationToken cancellationToken)
  {
    var client = factory.CreateClient();

    var response = await client.GetAsync("/openapi/v1.json", cancellationToken);
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
    var root = json.RootElement;

    var security = root
      .GetProperty("paths")
      .GetProperty("/api/v1/secure/ping")
      .GetProperty("get")
      .GetProperty("security");

    await Assert.That(security.GetArrayLength()).IsEqualTo(1);
    await Assert.That(security[0].TryGetProperty("oauth2", out _)).IsTrue();
  }

  [Test]
  public async Task Each_api_version_is_served_as_its_own_document(CancellationToken cancellationToken)
  {
    var client = factory.CreateClient();

    var v1 = await client.GetAsync("/openapi/v1.json", cancellationToken);
    var v2 = await client.GetAsync("/openapi/v2.json", cancellationToken);

    await Assert.That(v1.StatusCode).IsEqualTo(HttpStatusCode.OK);
    await Assert.That(v2.StatusCode).IsEqualTo(HttpStatusCode.OK);
  }

  [Test]
  public async Task Query_operation_is_injected_with_its_request_body(CancellationToken cancellationToken)
  {
    var client = factory.CreateClient();

    var response = await client.GetAsync("/openapi/v1.json", cancellationToken);
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
    var query = json.RootElement
      .GetProperty("paths")
      .GetProperty("/api/v1/pet")
      .GetProperty("query");

    await Assert.That(query.GetProperty("operationId").GetString()).IsEqualTo("SearchPetsV1");

    var schemaReference = query
      .GetProperty("requestBody")
      .GetProperty("content")
      .GetProperty("application/json")
      .GetProperty("schema")
      .GetProperty("$ref")
      .GetString();

    await Assert.That(schemaReference).IsEqualTo("#/components/schemas/PetSearchV1Dto");

    var itemsReference = query
      .GetProperty("responses")
      .GetProperty("200")
      .GetProperty("content")
      .GetProperty("application/json")
      .GetProperty("schema")
      .GetProperty("items")
      .GetProperty("$ref")
      .GetString();

    await Assert.That(itemsReference).IsEqualTo("#/components/schemas/PetV1Dto");
  }

  [Test]
  public async Task Query_operation_is_scoped_to_its_api_version(CancellationToken cancellationToken)
  {
    var client = factory.CreateClient();

    var response = await client.GetAsync("/openapi/v2.json", cancellationToken);
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

    // The QUERY endpoint is mapped to API version 1 only, so it must not leak into the v2 document.
    await Assert.That(json.RootElement.GetProperty("paths").TryGetProperty("/api/v1/pet", out _)).IsFalse();
  }
}
