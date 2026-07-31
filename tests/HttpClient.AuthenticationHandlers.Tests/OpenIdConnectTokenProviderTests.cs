namespace Kritikos.HttpClient.AuthenticationHandlers.Tests;

using System.ComponentModel.DataAnnotations;
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
    using var provider = CreateProvider(
      static request => request.Method == HttpMethod.Get
        ? Json(DiscoveryJson)
        : Json("""{"access_token":"test-token","token_type":"Bearer"}"""),
      TimeProvider.System);

    var token = await provider.GetAccessToken(cancellationToken);

    await Assert.That(token).IsEqualTo("test-token");
  }

  [Test]
  public async Task GetAccessToken_throws_when_the_discovery_document_request_fails(CancellationToken cancellationToken)
  {
    using var provider = CreateProvider(
      static _ => new HttpResponseMessage(HttpStatusCode.InternalServerError),
      TimeProvider.System);

    await Assert.That(async () => await provider.GetAccessToken(cancellationToken))
      .Throws<HttpRequestException>();
  }

  [Test]
  public async Task GetAccessToken_returns_empty_when_the_response_has_no_access_token(CancellationToken cancellationToken)
  {
    using var provider = CreateProvider(
      static request => request.Method == HttpMethod.Get
        ? Json(DiscoveryJson)
        : Json("""{"error":"invalid_client"}"""),
      TimeProvider.System);

    var token = await provider.GetAccessToken(cancellationToken);

    await Assert.That(token).IsEqualTo(string.Empty);
  }

  [Test]
  public async Task GetAccessToken_reuses_the_cached_token_until_it_nears_expiry(CancellationToken cancellationToken)
  {
    var tokenRequests = 0;
    using var provider = CreateProvider(
      request =>
      {
        if (request.Method == HttpMethod.Get)
        {
          return Json(DiscoveryJson);
        }

        Interlocked.Increment(ref tokenRequests);
        return Json("""{"access_token":"test-token","token_type":"Bearer","expires_in":3600}""");
      },
      TimeProvider.System);

    var first = await provider.GetAccessToken(cancellationToken);
    var second = await provider.GetAccessToken(cancellationToken);

    await Assert.That(first).IsEqualTo("test-token");
    await Assert.That(second).IsEqualTo("test-token");
    await Assert.That(tokenRequests).IsEqualTo(1);
  }

  [Test]
  public async Task GetAccessToken_requests_a_new_token_once_the_cached_one_expires(CancellationToken cancellationToken)
  {
    var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    var clock = Substitute.For<TimeProvider>();
    clock.GetUtcNow().Returns(_ => now);

    var tokenRequests = 0;
    using var provider = CreateProvider(
      request =>
      {
        if (request.Method == HttpMethod.Get)
        {
          return Json(DiscoveryJson);
        }

        var issued = Interlocked.Increment(ref tokenRequests);
        return Json($$"""{"access_token":"token-{{issued}}","token_type":"Bearer","expires_in":3600}""");
      },
      clock);

    var first = await provider.GetAccessToken(cancellationToken);
    now = now.AddHours(2);
    var second = await provider.GetAccessToken(cancellationToken);

    await Assert.That(first).IsEqualTo("token-1");
    await Assert.That(second).IsEqualTo("token-2");
    await Assert.That(tokenRequests).IsEqualTo(2);
  }

  [Test]
  public async Task GetAccessToken_fetches_the_discovery_document_only_once_across_refreshes(
    CancellationToken cancellationToken)
  {
    var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    var clock = Substitute.For<TimeProvider>();
    clock.GetUtcNow().Returns(_ => now);

    var discoveryRequests = 0;
    var tokenRequests = 0;
    using var provider = CreateProvider(
      request =>
      {
        if (request.Method == HttpMethod.Get)
        {
          Interlocked.Increment(ref discoveryRequests);
          return Json(DiscoveryJson);
        }

        Interlocked.Increment(ref tokenRequests);
        return Json("""{"access_token":"test-token","token_type":"Bearer","expires_in":3600}""");
      },
      clock);

    _ = await provider.GetAccessToken(cancellationToken);
    now = now.AddHours(2);
    _ = await provider.GetAccessToken(cancellationToken);

    await Assert.That(tokenRequests).IsEqualTo(2);
    await Assert.That(discoveryRequests).IsEqualTo(1);
  }

  [Test]
  public async Task GetAccessToken_coalesces_concurrent_callers_into_a_single_token_request(
    CancellationToken cancellationToken)
  {
    var firstRequestStarted = new TaskCompletionSource();
    var release = new TaskCompletionSource();
    var tokenRequests = 0;
    var handler = new AsyncStubHttpMessageHandler(async (request, ct) =>
    {
      if (request.Method == HttpMethod.Get)
      {
        return Json(DiscoveryJson);
      }

      if (Interlocked.Increment(ref tokenRequests) == 1)
      {
        firstRequestStarted.SetResult();
      }

      await release.Task.WaitAsync(ct);
      return Json("""{"access_token":"test-token","token_type":"Bearer","expires_in":3600}""");
    });
    using var provider = CreateProvider(handler, TimeProvider.System);

    var callers = Enumerable.Range(0, 8)
      .Select(_ => provider.GetAccessToken(cancellationToken))
      .ToArray();

    await firstRequestStarted.Task.WaitAsync(cancellationToken);
    release.SetResult();
    var tokens = await Task.WhenAll(callers);

    await Assert.That(tokens.All(static token => token == "test-token")).IsTrue();
    await Assert.That(tokenRequests).IsEqualTo(1);
  }

  [Test]
  public async Task Constructor_throws_when_options_fail_validation()
  {
    var factory = Substitute.For<IHttpClientFactory>();

    await Assert.That(() =>
      {
        _ = new OpenIdConnectTokenProvider(
          factory,
          new OpenIdConnectHandlerOptions(),
          TimeProvider.System,
          NullLogger<OpenIdConnectTokenProvider>.Instance);
      })
      .Throws<ValidationException>();
  }

  private static HttpResponseMessage Json(string content)
    => new(HttpStatusCode.OK) { Content = new StringContent(content, Encoding.UTF8, "application/json") };

  private static OpenIdConnectTokenProvider CreateProvider(
    Func<HttpRequestMessage, HttpResponseMessage> responder,
    TimeProvider timeProvider)
    => CreateProvider(new StubHttpMessageHandler(responder), timeProvider);

  private static OpenIdConnectTokenProvider CreateProvider(
    HttpMessageHandler handler,
    TimeProvider timeProvider)
  {
    var factory = Substitute.For<IHttpClientFactory>();
    factory.CreateClient(Arg.Any<string>()).Returns(new System.Net.Http.HttpClient(handler));

    var options = new OpenIdConnectHandlerOptions
    {
      WellKnownEndpoint = new Uri("https://issuer.example/.well-known/openid-configuration"),
      ClientId = "client",
      ClientSecret = "secret",
    };

    return new OpenIdConnectTokenProvider(
      factory,
      options,
      timeProvider,
      NullLogger<OpenIdConnectTokenProvider>.Instance);
  }

  private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    : HttpMessageHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken cancellationToken)
      => Task.FromResult(responder(request));
  }

  private sealed class AsyncStubHttpMessageHandler(
    Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
    : HttpMessageHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken cancellationToken)
      => responder(request, cancellationToken);
  }
}
