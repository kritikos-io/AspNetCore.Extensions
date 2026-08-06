namespace Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;

using System.ComponentModel.DataAnnotations;

using Kritikos.Extensions.Options.Contracts;

/// <summary>
/// Configuration for the RFC 9728 <c>/.well-known/oauth-protected-resource</c> endpoint.
/// </summary>
public sealed class OAuthProtectedResourceOptions : IOptionsDefinition, IValidatableObject
{
  /// <inheritdoc />
  public static string Location { get; } = "AspNetCore:WellKnown:OAuthProtectedResource";

  /// <summary>
  /// Gets or sets the protected resource's identifier: an absolute https URI without a fragment. When unset,
  /// the endpoint uses the request's base URL.
  /// </summary>
  public Uri? Resource { get; set; }

  /// <summary>Gets the issuer identifiers of authorization servers that can be used with this resource.</summary>
  public IList<Uri> AuthorizationServers { get; } = [];

  /// <summary>Gets the OAuth 2.0 scope values used to request access to this resource.</summary>
  public IList<string> ScopesSupported { get; } = [];

  /// <summary>Gets the supported bearer token presentation methods (<c>header</c>, <c>body</c>, or <c>query</c>).</summary>
  public IList<string> BearerMethodsSupported { get; } = [];

  /// <summary>Gets or sets the URL of the resource's JSON Web Key Set document.</summary>
  public Uri? JwksUri { get; set; }

  /// <summary>Gets or sets the URL of human-readable documentation for the resource.</summary>
  public Uri? ResourceDocumentation { get; set; }

  /// <summary>Gets or sets the human-readable name of the protected resource.</summary>
  public string? ResourceName { get; set; }

  /// <inheritdoc />
  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (Resource is not null
        && (!Resource.IsAbsoluteUri || Resource.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(Resource.Fragment)))
    {
      yield return new("Resource, when set, must be an absolute https URI without a fragment.", [nameof(Resource)]);
    }

    foreach (var method in BearerMethodsSupported)
    {
      if (method is not ("header" or "body" or "query"))
      {
        yield return new("BearerMethodsSupported values must be one of: header, body, query.", [nameof(BearerMethodsSupported)]);
      }
    }

    if (JwksUri is not null && (!JwksUri.IsAbsoluteUri || JwksUri.Scheme != Uri.UriSchemeHttps))
    {
      yield return new("JwksUri must be an absolute https URI.", [nameof(JwksUri)]);
    }
  }
}
