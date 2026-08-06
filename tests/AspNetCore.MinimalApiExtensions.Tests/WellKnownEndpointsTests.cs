#pragma warning disable CA2234 // Pass System.Uri objects instead of strings

namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using System.Net;
using System.Text.Json;

using Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public sealed class WellKnownEndpointsTests
{
  [Test]
  public async Task Security_txt_is_served_as_plain_text(CancellationToken cancellationToken)
  {
    await using var app = await StartHostAsync();
    using var client = app.GetTestClient();

    var response = await client.GetAsync("/.well-known/security.txt", cancellationToken);
    var body = await response.Content.ReadAsStringAsync(cancellationToken);

    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("text/plain");
    await Assert.That(body).Contains("Contact: mailto:security@example.com");
    await Assert.That(body).Contains("Expires: 2030-01-01T00:00:00Z");
  }

  [Test]
  public async Task Protected_resource_metadata_is_served_as_json(CancellationToken cancellationToken)
  {
    await using var app = await StartHostAsync();
    using var client = app.GetTestClient();

    var response = await client.GetAsync("/.well-known/oauth-protected-resource", cancellationToken);
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);

    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("application/json");
    await Assert.That(document.RootElement.GetProperty("resource").GetString())
      .IsEqualTo("https://resource.example.com");
    await Assert.That(document.RootElement.TryGetProperty("authorization_servers", out _)).IsTrue();
  }

  [Test]
  public async Task Api_catalog_is_served_as_linkset(CancellationToken cancellationToken)
  {
    await using var app = await StartHostAsync();
    using var client = app.GetTestClient();

    var response = await client.GetAsync("/.well-known/api-catalog", cancellationToken);
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);

    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("application/linkset+json");
    await Assert.That(document.RootElement.GetProperty("linkset").GetArrayLength()).IsEqualTo(1);
    await Assert.That(response.Headers.TryGetValues("Link", out var link)).IsTrue();
    await Assert.That(string.Join(string.Empty, link!)).Contains("rel=\"api-catalog\"");
  }

  [Test]
  public async Task Api_catalog_composes_registered_sources(CancellationToken cancellationToken)
  {
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();
    builder.Logging.ClearProviders();

    builder.Services.AddSingleton<IApiCatalogSource, StubCatalogSource>();
    builder.Services.AddSingleton<IApiCatalogSource, ExternalCatalogSource>();

    await using var app = builder.Build();
    app.MapApiCatalog();
    await app.StartAsync(cancellationToken);
    using var client = app.GetTestClient();

    var response = await client.GetAsync("/.well-known/api-catalog", cancellationToken);
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);

    var anchors = document.RootElement.GetProperty("linkset").EnumerateArray()
      .Select(entry => entry.GetProperty("anchor").GetString())
      .ToArray();

    await Assert.That(anchors).Contains("https://api.example.com/discovered");
    await Assert.That(anchors).Contains("https://api.example.com/external");
  }

  [Test]
  public async Task Protected_resource_defaults_to_request_base_url_when_unset(CancellationToken cancellationToken)
  {
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();
    builder.Logging.ClearProviders();

    builder.Services.AddOAuthProtectedResource(options => options.ScopesSupported.Add("openid"));

    await using var app = builder.Build();
    app.MapOAuthProtectedResource();
    await app.StartAsync(cancellationToken);
    using var client = app.GetTestClient();

    var response = await client.GetAsync("/.well-known/oauth-protected-resource", cancellationToken);
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);

    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    await Assert.That(document.RootElement.GetProperty("resource").GetString()).IsEqualTo("http://localhost");
  }

  private static async Task<WebApplication> StartHostAsync()
  {
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();
    builder.Logging.ClearProviders();

    builder.Services.AddSecurityTxt(options =>
    {
      options.Contact.Add(new Uri("mailto:security@example.com"));
      options.Expires = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
    });
    builder.Services.AddOAuthProtectedResource(options =>
    {
      options.Resource = new Uri("https://resource.example.com");
      options.AuthorizationServers.Add(new Uri("https://as.example.com"));
      options.ScopesSupported.Add("openid");
    });
    builder.Services.AddSingleton<IApiCatalogSource, StubCatalogSource>();

    var app = builder.Build();
    app.MapSecurityTxt();
    app.MapOAuthProtectedResource();
    app.MapApiCatalog();

    await app.StartAsync();
    return app;
  }

  private sealed class StubCatalogSource : IApiCatalogSource
  {
    public IEnumerable<ApiCatalogEntry> GetEntries(HttpContext context) =>
    [
      new()
      {
        Anchor = new Uri("https://api.example.com/discovered"),
        ServiceDescription = new Uri("https://api.example.com/discovered/openapi.json"),
      },
    ];
  }

  private sealed class ExternalCatalogSource : IApiCatalogSource
  {
    public IEnumerable<ApiCatalogEntry> GetEntries(HttpContext context) =>
    [
      new()
      {
        Anchor = new Uri("https://api.example.com/external"),
        ServiceDescription = new Uri("https://api.example.com/external/openapi.json"),
      },
    ];
  }
}
