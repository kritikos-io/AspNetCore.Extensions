namespace Kritikos.AspNetCore.OpenApiExtensions;

using Microsoft.AspNetCore.Authorization;

using Microsoft.OpenApi;

/// <summary>
/// Shared translation of an endpoint's authorization metadata into OpenAPI security requirements and the
/// conventional 401/403 responses, used by both the operation- and document-level transformers.
/// </summary>
internal static class OperationSecurity
{
  /// <summary>
  /// Determines whether the endpoint is secured: it declares <see cref="IAuthorizeData"/> and is not overridden by
  /// <c>[AllowAnonymous]</c>, which wins at runtime.
  /// </summary>
  /// <param name="metadata">The endpoint metadata.</param>
  /// <param name="authorization">The authorization data declared on the endpoint, if any.</param>
  /// <returns><see langword="true"/> when the endpoint requires authorization.</returns>
  public static bool IsSecured(IEnumerable<object> metadata, out IAuthorizeData[] authorization)
  {
    authorization = [.. metadata.OfType<IAuthorizeData>()];
    return authorization.Length > 0 && !metadata.OfType<IAllowAnonymous>().Any();
  }

  /// <summary>
  /// Adds the conventional 401/403 responses and one security requirement per resolved scheme to the operation.
  /// </summary>
  /// <param name="operation">The operation to annotate.</param>
  /// <param name="document">The document the security-scheme references resolve against.</param>
  /// <param name="metadata">The endpoint metadata.</param>
  /// <param name="authorization">The authorization data declared on the endpoint.</param>
  /// <param name="defaultSchemeId">The fallback security-scheme key when no authentication scheme is declared.</param>
  /// <param name="schemesByAuthenticationScheme">Map from authentication scheme name to OpenAPI security-scheme key.</param>
  public static void Apply(
    OpenApiOperation operation,
    OpenApiDocument? document,
    IEnumerable<object> metadata,
    IAuthorizeData[] authorization,
    string defaultSchemeId,
    IReadOnlyDictionary<string, string> schemesByAuthenticationScheme)
  {
    operation.Responses ??= [];
    operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
    operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

    operation.Security =
    [
      .. ResolveSchemeIds(metadata, authorization, defaultSchemeId, schemesByAuthenticationScheme)
        .Select(id => new OpenApiSecurityRequirement
        {
          [new OpenApiSecuritySchemeReference(id, document)] = [],
        }),
    ];
  }

  private static IEnumerable<string> ResolveSchemeIds(
    IEnumerable<object> metadata,
    IAuthorizeData[] authorization,
    string defaultSchemeId,
    IReadOnlyDictionary<string, string> schemesByAuthenticationScheme)
  {
    var authenticationSchemes = metadata.OfType<AuthorizationPolicy>()
      .SelectMany(policy => policy.AuthenticationSchemes)
      .Concat(authorization
        .Select(data => data.AuthenticationSchemes)
        .Where(schemes => !string.IsNullOrWhiteSpace(schemes))
        .SelectMany(schemes =>
          schemes!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)))
      .Distinct(StringComparer.Ordinal)
      .ToArray();

    return authenticationSchemes.Length == 0
      ? [defaultSchemeId]
      : authenticationSchemes
        .Select(scheme => schemesByAuthenticationScheme.GetValueOrDefault(scheme, defaultSchemeId))
        .Distinct(StringComparer.Ordinal);
  }
}
