namespace Kritikos.AspNetCore.OpenApiExtensions;

using Kritikos.AspNetCore.OpenApiExtensions.DocumentTransformers;

using Microsoft.AspNetCore.OpenApi;

/// <summary>
/// OpenAPI configuration extensions for declaring security schemes: a bare HTTP bearer (JWT) scheme, an OAuth2
/// authorization-code scheme that also advertises the interactive login flow, or an API-key header scheme.
/// </summary>
public static class KritikosOpenApiOpenApiOptionsExtensions
{
  /// <param name="options">The <see cref="OpenApiOptions"/> to add the transformer to.</param>
  extension(OpenApiOptions options)
  {
    /// <summary>
    /// Declares the HTTP bearer (JWT) security scheme the API validates, under the key <c>bearer</c>. This describes
    /// the token the resource server accepts; interactive login is an API-reference-UI concern configured separately.
    /// </summary>
    /// <returns>The configured <see cref="OpenApiOptions"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public OpenApiOptions AddBearerSecurityScheme()
    {
      ArgumentNullException.ThrowIfNull(options);

      return options.AddDocumentTransformer(new BearerSecuritySchemeDocumentTransformer());
    }

    /// <summary>
    /// Declares an OAuth2 authorization-code security scheme with inline authorization and token endpoints, under the
    /// key <c>oauth2</c>. This advertises the interactive flow an API-reference UI needs to obtain a bearer token;
    /// point <see cref="OperationTransformers.AuthorizationCheckOperationTransformer"/> at the same key.
    /// </summary>
    /// <param name="authorizationUrl">The authorization-code flow authorization endpoint.</param>
    /// <param name="tokenUrl">The authorization-code flow token endpoint.</param>
    /// <param name="scopes">The scopes advertised by the flow, keyed by scope name with a human-readable description.</param>
    /// <returns>The configured <see cref="OpenApiOptions"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/>, <paramref name="authorizationUrl"/>, or <paramref name="tokenUrl"/> is <see langword="null"/>.</exception>
    public OpenApiOptions AddOAuth2SecurityScheme(
      Uri authorizationUrl,
      Uri tokenUrl,
      IReadOnlyDictionary<string, string>? scopes = null)
    {
      ArgumentNullException.ThrowIfNull(options);

      return options.AddDocumentTransformer(
        new OAuth2SecuritySchemeDocumentTransformer(authorizationUrl, tokenUrl, scopes));
    }

    /// <summary>
    /// Declares an API-key security scheme carried in the <paramref name="headerName"/> request header, under the key
    /// <c>apiKey</c>; point <see cref="OperationTransformers.AuthorizationCheckOperationTransformer"/> at the same key.
    /// </summary>
    /// <param name="headerName">The request header carrying the API key (for example, <c>X-API-Key</c>).</param>
    /// <returns>The configured <see cref="OpenApiOptions"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="headerName"/> is null or whitespace.</exception>
    public OpenApiOptions AddApiKeySecurityScheme(string headerName)
    {
      ArgumentNullException.ThrowIfNull(options);

      return options.AddDocumentTransformer(new ApiKeySecuritySchemeDocumentTransformer(headerName));
    }

    /// <summary>
    /// Re-injects HTTP <c>QUERY</c> operations that the built-in generator drops (OpenAPI 3.1 has no <c>query</c>
    /// path-item field). Each described QUERY endpoint is added back with its request body, parameters, responses,
    /// and \u2014 for secured endpoints \u2014 401/403 responses plus a security requirement.
    /// </summary>
    /// <param name="securitySchemeId">The default security-scheme key for secured QUERY operations that declare no authentication scheme.</param>
    /// <param name="schemesByAuthenticationScheme">Map from authentication scheme name to OpenAPI security-scheme key.</param>
    /// <returns>The configured <see cref="OpenApiOptions"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public OpenApiOptions AddQueryOperations(
      string securitySchemeId = "bearer",
      IReadOnlyDictionary<string, string>? schemesByAuthenticationScheme = null)
    {
      ArgumentNullException.ThrowIfNull(options);

      return options.AddDocumentTransformer(
        new QueryOperationDocumentTransformer(securitySchemeId, schemesByAuthenticationScheme));
    }
  }
}
