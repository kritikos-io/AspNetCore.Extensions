namespace Kritikos.HttpClient.AuthenticationHandlers;

using System.Net.Http.Json;

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

public partial class OpenIdConnectTokenProvider(
  IHttpClientFactory clientFactory,
  OpenIdConnectHandlerOptions options,
  ILogger<OpenIdConnectTokenProvider> logger)
{
  private readonly OpenIdConnectHandlerOptions options = options;
  private readonly IHttpClientFactory clientFactory = clientFactory;

  private readonly List<KeyValuePair<string, string>> parameters =
  [
    new("grant_type", "client_credentials"),
    new("client_id", options.ClientId),
    new("client_secret", options.ClientSecret),
    new("scope", "offline_access")
  ];

  private OpenIdConnectConfiguration? discoveryDocument;

  public async Task<string> GetAccessToken(CancellationToken cancellationToken = default)
  {
    var discovery = await GetDiscoveryDocument(cancellationToken);
    using var request = new HttpRequestMessage(HttpMethod.Post, discovery.TokenEndpoint);
    request.Content = new FormUrlEncodedContent(parameters);

    var response = await clientFactory.CreateClient(nameof(OpenIdConnectTokenProvider))
      .SendAsync(request, cancellationToken);
    var tokenDictionary = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(cancellationToken) ?? [];
    if (!tokenDictionary.TryGetValue("access_token", out var token))
    {
      ErrorCreatingAccessToken(logger, request.RequestUri, response.ReasonPhrase);
      return string.Empty;
    }

    return token as string ?? string.Empty;
  }

  private async ValueTask<OpenIdConnectConfiguration> GetDiscoveryDocument(
    CancellationToken cancellationToken = default)
  {
    if (discoveryDocument is not null)
    {
      return discoveryDocument;
    }

    var client = clientFactory.CreateClient(nameof(OpenIdConnectTokenProvider));
    var response = await client.GetAsync(options.WellKnownEndpoint, cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
      ErrorFetchingDiscoveryDocument(logger, options.WellKnownEndpoint, response.ReasonPhrase);
    }

    var content = await response.Content.ReadAsStringAsync(cancellationToken);
    discoveryDocument = OpenIdConnectConfiguration.Create(content);
    return discoveryDocument;
  }

  [LoggerMessage(LogLevel.Error, "Error fetching discovery document from {Url}: {Error}")]
  private static partial void ErrorFetchingDiscoveryDocument(ILogger logger, Uri url, string? error);

  [LoggerMessage(LogLevel.Error, "Error fetching discovery document from {Url}: {Error}")]
  private static partial void ErrorCreatingAccessToken(ILogger logger, Uri? url, string? error);
}
