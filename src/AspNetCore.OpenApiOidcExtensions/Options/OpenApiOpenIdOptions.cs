namespace Kritikos.AspNetCore.OpenApiOidcExtensions.Options;

/// <summary>
/// Base configuration options for OpenID Connect integration with OpenAPI.
/// </summary>
public abstract class OpenApiOpenIdOptions
{
  /// <summary>
  /// Gets or sets the OpenID Connect authority URL.
  /// </summary>
  public virtual string Authority { get; set; } = string.Empty;
}
