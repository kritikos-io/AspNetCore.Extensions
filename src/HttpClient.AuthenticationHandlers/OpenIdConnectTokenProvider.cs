namespace Kritikos.HttpClient.AuthenticationHandlers;

using System.Net.Http.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

/// <summary>
/// Provides access tokens using the OAuth 2.0 client credentials flow via an OpenID Connect provider.
/// </summary>
/// <remarks>
/// Access tokens are cached and reused until shortly before they expire, and both the token refresh and the
/// discovery-document fetch are guarded by single-flight primitives so concurrent callers trigger at most one
/// request to the identity provider.
/// </remarks>
/// <param name="clientFactory">The HTTP client factory used to create clients for token requests.</param>
/// <param name="options">The OpenID Connect handler options containing endpoint and credential information.</param>
/// <param name="timeProvider">The clock used to evaluate token expiry.</param>
/// <param name="logger">The logger instance used for error logging.</param>
public sealed partial class OpenIdConnectTokenProvider(
  IHttpClientFactory clientFactory,
  OpenIdConnectHandlerOptions options,
  TimeProvider timeProvider,
  ILogger<OpenIdConnectTokenProvider> logger) : IDisposable
{
  private static readonly TimeSpan TokenRefreshMargin = TimeSpan.FromSeconds(30);

  private readonly OpenIdConnectHandlerOptions options = options;
  private readonly IHttpClientFactory clientFactory = clientFactory;
  private readonly TimeProvider timeProvider = timeProvider;
  private readonly SemaphoreSlim tokenGate = new(1, 1);
  private readonly SemaphoreSlim discoveryGate = new(1, 1);

  private readonly List<KeyValuePair<string, string>> parameters =
  [
    new("grant_type", "client_credentials"),
    new("client_id", options.ClientId),
    new("client_secret", options.ClientSecret),
  ];

  private OpenIdConnectConfiguration? discoveryDocument;
  private CachedToken? cache;

  /// <summary>
  /// Retrieves an access token from the OpenID Connect token endpoint using client credentials.
  /// </summary>
  /// <remarks>
  /// A previously obtained token is reused until it is within the refresh margin of its expiry; only then is a
  /// new token requested. Concurrent callers share a single refresh rather than each issuing a token request.
  /// </remarks>
  /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
  /// <returns>The access token string, or <see cref="string.Empty"/> if the token could not be obtained.</returns>
  public async Task<string> GetAccessToken(CancellationToken cancellationToken = default)
  {
    if (GetCachedToken() is { } fresh)
    {
      return fresh;
    }

    await tokenGate.WaitAsync(cancellationToken);
    try
    {
      if (GetCachedToken() is { } current)
      {
        return current;
      }

      var discovery = await GetDiscoveryDocument(cancellationToken);
      using var request = new HttpRequestMessage(HttpMethod.Post, discovery.TokenEndpoint)
      {
        Content = new FormUrlEncodedContent(parameters),
      };

      using var response = await clientFactory.CreateClient(nameof(OpenIdConnectTokenProvider))
        .SendAsync(request, cancellationToken);
      var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
      if (tokenResponse is null || string.IsNullOrEmpty(tokenResponse.AccessToken))
      {
        ErrorCreatingAccessToken(logger, request.RequestUri, response.ReasonPhrase);
        return string.Empty;
      }

      var expiresAt = timeProvider.GetUtcNow() + TimeSpan.FromSeconds(tokenResponse.ExpiresIn);
      cache = new CachedToken(tokenResponse.AccessToken, expiresAt);
      return tokenResponse.AccessToken;
    }
    finally
    {
      tokenGate.Release();
    }
  }

  /// <inheritdoc />
  public void Dispose()
  {
    tokenGate.Dispose();
    discoveryGate.Dispose();
  }

  [LoggerMessage(LogLevel.Error, "Error fetching discovery document from {Url}: {Error}")]
  private static partial void ErrorFetchingDiscoveryDocument(ILogger logger, Uri url, string? error);

  [LoggerMessage(LogLevel.Error, "Error creating access token from {Url}: {Error}")]
  private static partial void ErrorCreatingAccessToken(ILogger logger, Uri? url, string? error);

  private string? GetCachedToken()
  {
    var snapshot = cache;
    return snapshot is not null && timeProvider.GetUtcNow() + TokenRefreshMargin < snapshot.ExpiresAt
      ? snapshot.Value
      : null;
  }

  private async ValueTask<OpenIdConnectConfiguration> GetDiscoveryDocument(
    CancellationToken cancellationToken = default)
  {
    if (discoveryDocument is not null)
    {
      return discoveryDocument;
    }

    await discoveryGate.WaitAsync(cancellationToken);
    try
    {
      if (discoveryDocument is not null)
      {
        return discoveryDocument;
      }

      var client = clientFactory.CreateClient(nameof(OpenIdConnectTokenProvider));
      using var response = await client.GetAsync(options.WellKnownEndpoint, cancellationToken);
      if (!response.IsSuccessStatusCode)
      {
        ErrorFetchingDiscoveryDocument(logger, options.WellKnownEndpoint, response.ReasonPhrase);
        response.EnsureSuccessStatusCode();
      }

      var content = await response.Content.ReadAsStringAsync(cancellationToken);
      discoveryDocument = OpenIdConnectConfiguration.Create(content);
      return discoveryDocument;
    }
    finally
    {
      discoveryGate.Release();
    }
  }

  private sealed record CachedToken(string Value, DateTimeOffset ExpiresAt);

  private sealed record TokenResponse
  {
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }

    [JsonPropertyName("expires_in")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int ExpiresIn { get; init; }
  }
}
