namespace Kritikos.HttpClient.AuthenticationHandlers;

/// <summary>
/// Configuration options for the OpenID Connect client credentials handler.
/// </summary>
public class OpenIdConnectHandlerOptions
{
  /// <summary>
  /// Gets or sets the URI of the OpenID Connect well-known discovery endpoint.
  /// </summary>
  public Uri WellKnownEndpoint { get; set; } = new Uri("about:blank");

  /// <summary>
  /// Gets or sets the client identifier for authentication.
  /// </summary>
  public string ClientId { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the client secret for authentication.
  /// </summary>
  public string ClientSecret { get; set; } = string.Empty;
}
