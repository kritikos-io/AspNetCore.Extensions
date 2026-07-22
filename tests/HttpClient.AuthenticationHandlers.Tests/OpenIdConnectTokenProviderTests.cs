namespace Kritikos.HttpClient.AuthenticationHandlers.Tests;

using System.Net;
using System.Text;

using Kritikos.HttpClient.AuthenticationHandlers;

using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

public class OpenIdConnectTokenProviderTests
{
  private const string DiscoveryJson =
    """{"issuer":"https://issuer.example","token_endpoint":"https://issuer.example/connect/token","jwks_uri":"https://issuer.example/jwks"}""";

  [Test]
  public async Task GetAccessToken_returns_the_token_on_success(CancellationToken cancellationToken)
  {
    var provider = CreateProvider(static request => request.Method == HttpMethod.Get
      ? Json(DiscoveryJson)
      : Json("""{"access_token":"test-token","token_type":"Bearer"}"""));

    var token = await provider.GetAccessToken(cancellationToken);

    await Assert.That(token).IsEqualTo("test-token");
  }

  [Test]
  public async Task GetAccessToken_throws_when_the_discovery_document_request_fails(CancellationToken cancellationToken)
  {
    var provider = CreateProvider(static _ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

    await Assert.That(async () => await provider.GetAccessToken(cancellationToken))
      .Throws<HttpRequestException>();
  }

  [Test]
  public async Task GetAccessToken_returns_empty_when_the_response_has_no_access_token(CancellationToken cancellationToken)
  {
    var provider = CreateProvider(static request => request.Method == HttpMethod.Get
      ? Json(DiscoveryJson)
      : Json("""{"error":"invalid_client"}"""));

    var token = await provider.GetAccessToken(cancellationToken);

    await Assert.That(token).IsEqualTo(string.Empty);
  }

  private static HttpResponseMessage Json(string content)
    => new(HttpStatusCode.OK) { Content = new StringContent(content, Encoding.UTF8, "application/json") };

  private static OpenIdConnectTokenProvider CreateProvider(Func<HttpRequestMessage, HttpResponseMessage> responder)
  {
    var handler = new StubHttpMessageHandler(responder);
    var factory = Substitute.For<IHttpClientFactory>();
    factory.CreateClient(Arg.Any<string>()).Returns(new System.Net.Http.HttpClient(handler));

    var options = new OpenIdConnectHandlerOptions
    {
      WellKnownEndpoint = new Uri("https://issuer.example/.well-known/openid-configuration"),
      ClientId = "client",
      ClientSecret = "secret",
    };

    return new OpenIdConnectTokenProvider(factory, options, NullLogger<OpenIdConnectTokenProvider>.Instance);
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
