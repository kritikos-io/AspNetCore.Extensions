namespace Kritikos.AspNetCore.MinimalApiExtensions.Authentication;

using Microsoft.AspNetCore.Authentication;

/// <summary>
/// Options for <see cref="ApiKeyAuthenticationHandler"/>.
/// </summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
  /// <summary>
  /// Gets or sets the request header carrying the API key. Defaults to <see cref="ApiKeyDefaults.HeaderName"/>.
  /// </summary>
  public string HeaderName { get; set; } = ApiKeyDefaults.HeaderName;
}
