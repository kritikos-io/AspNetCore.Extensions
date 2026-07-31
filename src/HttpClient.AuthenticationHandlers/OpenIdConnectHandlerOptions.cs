namespace Kritikos.HttpClient.AuthenticationHandlers;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Configuration options for the OpenID Connect client credentials handler.
/// </summary>
public sealed class OpenIdConnectHandlerOptions : IValidatableObject
{
  /// <summary>
  /// Gets or sets the URI of the OpenID Connect well-known discovery endpoint.
  /// </summary>
  public Uri WellKnownEndpoint { get; set; } = new Uri("about:blank");

  /// <summary>
  /// Gets or sets the OAuth 2.0 client identifier used for the client credentials flow.
  /// </summary>
  public string ClientId { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the OAuth 2.0 client secret used for the client credentials flow.
  /// </summary>
  public string ClientSecret { get; set; } = string.Empty;

  /// <inheritdoc />
  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (WellKnownEndpoint is not { IsAbsoluteUri: true, Scheme: "https" or "http" })
    {
      yield return new ValidationResult(
        "WellKnownEndpoint must be an absolute http or https URI.",
        [nameof(WellKnownEndpoint)]);
    }

    if (string.IsNullOrWhiteSpace(ClientId))
    {
      yield return new ValidationResult(
        "ClientId must not be empty or whitespace.",
        [nameof(ClientId)]);
    }

    if (string.IsNullOrWhiteSpace(ClientSecret))
    {
      yield return new ValidationResult(
        "ClientSecret must not be empty or whitespace.",
        [nameof(ClientSecret)]);
    }
  }
}
