namespace Kritikos.AspNetCore.OpenApiOidcExtensions.Tests;

using System.Net;
using System.Text;

using Kritikos.AspNetCore.OpenApiOidcExtensions.DocumentTransformers;
using Kritikos.AspNetCore.OpenApiOidcExtensions.Options;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

using NSubstitute;

public class OidcSecuritySchemeTransformerTests
{
  private const string DiscoveryJson =
    """
    {
      "issuer": "https://demo.identity",
      "authorization_endpoint": "https://demo.identity/connect/authorize",
      "token_endpoint": "https://demo.identity/connect/token"
    }
    """;

  [Test]
  public async Task TransformAsync_null_document_throws()
  {
    var transformer = CreateTransformer(static _ => Json(DiscoveryJson));

    await Assert.That(async () => await transformer.TransformAsync(null!, CreateContext(), CancellationToken.None))
      .Throws<ArgumentNullException>();
  }

  [Test]
  public async Task Discovery_document_adds_oauth2_scheme_with_flow_endpoints()
  {
    var transformer = CreateTransformer(static _ => Json(DiscoveryJson));
    var document = new OpenApiDocument();

    await transformer.TransformAsync(document, CreateContext(), CancellationToken.None);

    await Assert.That(document.Components!.SecuritySchemes!.ContainsKey("openid")).IsTrue();
    var scheme = (OpenApiSecurityScheme)document.Components.SecuritySchemes["openid"];
    await Assert.That(scheme.Type == SecuritySchemeType.OAuth2).IsTrue();
    await Assert.That(scheme.Flows!.AuthorizationCode!.AuthorizationUrl)
      .IsEqualTo(new Uri("https://demo.identity/connect/authorize"));
    await Assert.That(scheme.Flows.AuthorizationCode.TokenUrl)
      .IsEqualTo(new Uri("https://demo.identity/connect/token"));
    await Assert.That(scheme.Flows.AuthorizationCode.Scopes!.ContainsKey("openid")).IsTrue();
  }

  [Test]
  public async Task Security_requirement_is_appended_to_document()
  {
    var transformer = CreateTransformer(static _ => Json(DiscoveryJson));
    var document = new OpenApiDocument();

    await transformer.TransformAsync(document, CreateContext(), CancellationToken.None);

    await Assert.That(document.Security!.Count).IsEqualTo(1);
  }

  [Test]
  public async Task Failed_discovery_request_throws()
  {
    var transformer = CreateTransformer(static _ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
    var document = new OpenApiDocument { Components = new OpenApiComponents() };

    await Assert.That(async () => await transformer.TransformAsync(document, CreateContext(), CancellationToken.None))
      .Throws<HttpRequestException>();
  }

  // ---- Helpers ----
  private static HttpResponseMessage Json(string content)
    => new(HttpStatusCode.OK) { Content = new StringContent(content, Encoding.UTF8, "application/json") };

  private static OidcSecuritySchemeTransformer<TestOpenIdOptions> CreateTransformer(
    Func<HttpRequestMessage, HttpResponseMessage> responder)
  {
    var factory = Substitute.For<IHttpClientFactory>();
    factory.CreateClient(Arg.Any<string>())
      .Returns(_ => new System.Net.Http.HttpClient(new StubHttpMessageHandler(responder)));
    var options = Options.Create(new TestOpenIdOptions { Authority = "https://demo.identity" });

    return new OidcSecuritySchemeTransformer<TestOpenIdOptions>(options, factory);
  }

  private static OpenApiDocumentTransformerContext CreateContext()
    => new()
    {
      DocumentName = "v1",
      DescriptionGroups = [],
      ApplicationServices = Substitute.For<IServiceProvider>(),
    };

  private sealed class TestOpenIdOptions : OpenApiOpenIdOptions
  {
  }

  private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    : HttpMessageHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken cancellationToken)
      => Task.FromResult(responder(request));
  }
}
