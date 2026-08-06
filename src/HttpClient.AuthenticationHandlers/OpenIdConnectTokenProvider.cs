namespace Kritikos.HttpClient.AuthenticationHandlers;

using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

/// <summary>
/// Provides access tokens using the OAuth 2.0 client credentials flow via an OpenID Connect provider.
/// </summary>
/// <remarks>
/// Access tokens are cached and reused until shortly before they expire; the token refresh is guarded by a
/// single-flight primitive, and the OIDC discovery document is fetched, cached, and periodically refreshed via
/// <see cref="ConfigurationManager{T}"/>.
/// </remarks>
public sealed partial class OpenIdConnectTokenProvider : IDisposable
{
  private static readonly TimeSpan TokenRefreshMargin = TimeSpan.FromSeconds(30);

  private readonly IHttpClientFactory clientFactory;
  private readonly TimeProvider timeProvider;
  private readonly ILogger<OpenIdConnectTokenProvider> logger;
  private readonly SemaphoreSlim tokenGate = new(1, 1);
  private readonly ConfigurationManager<OpenIdConnectConfiguration> configurationManager;
  private readonly List<KeyValuePair<string, string>> parameters;

  private CachedToken? cache;

  /// <summary>
  /// Initializes a new instance of the <see cref="OpenIdConnectTokenProvider"/> class.
  /// </summary>
  /// <param name="clientFactory">The HTTP client factory used to create clients for token and discovery requests.</param>
  /// <param name="options">The OpenID Connect handler options containing endpoint and credential information.</param>
  /// <param name="timeProvider">The clock used to evaluate token expiry.</param>
  /// <param name="logger">The logger instance used for error logging.</param>
  /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
  public OpenIdConnectTokenProvider(
    IHttpClientFactory clientFactory,
    OpenIdConnectHandlerOptions options,
    TimeProvider timeProvider,
    ILogger<OpenIdConnectTokenProvider> logger)
  {
    ArgumentNullException.ThrowIfNull(options);
    Validator.ValidateObject(options, new ValidationContext(options), validateAllProperties: true);

    this.clientFactory = clientFactory;
    this.timeProvider = timeProvider;
    this.logger = logger;
    this.configurationManager = new(
      options.WellKnownEndpoint.AbsoluteUri,
      new OpenIdConnectConfigurationRetriever(),
      clientFactory.CreateClient(nameof(OpenIdConnectTokenProvider)));
    this.parameters =
    [
      new("grant_type", "client_credentials"),
      new("client_id", options.ClientId),
      new("client_secret", options.ClientSecret),
    ];
  }

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

      var discovery = await configurationManager.GetConfigurationAsync(cancellationToken);
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
  public void Dispose() => tokenGate.Dispose();

  [LoggerMessage(LogLevel.Error, "Error creating access token from {Url}: {Error}")]
  private static partial void ErrorCreatingAccessToken(ILogger logger, Uri? url, string? error);

  private string? GetCachedToken()
  {
    var snapshot = cache;
    return snapshot is not null && timeProvider.GetUtcNow() + TokenRefreshMargin < snapshot.ExpiresAt
      ? snapshot.Value
      : null;
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
