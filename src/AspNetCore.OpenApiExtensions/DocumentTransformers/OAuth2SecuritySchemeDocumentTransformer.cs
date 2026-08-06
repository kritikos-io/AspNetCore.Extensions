namespace Kritikos.AspNetCore.OpenApiExtensions.DocumentTransformers;

using Microsoft.AspNetCore.OpenApi;

using Microsoft.OpenApi;

/// <summary>
/// An OpenAPI document transformer that declares an OAuth2 authorization-code security scheme with inline
/// authorization and token endpoints, under the key <c>oauth2</c> referenced by
/// <see cref="OperationTransformers.AuthorizationCheckOperationTransformer"/>. Unlike a bare bearer scheme, this
/// advertises the interactive flow an API-reference UI (e.g. Scalar) needs to obtain a token.
/// </summary>
public sealed class OAuth2SecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer
{
  /// <summary>The security-scheme key this transformer declares.</summary>
  public const string SchemeId = "oauth2";

  private readonly Uri authorizationUrl;
  private readonly Uri tokenUrl;
  private readonly IReadOnlyDictionary<string, string> scopes;

  /// <summary>
  /// Initializes a new instance of the <see cref="OAuth2SecuritySchemeDocumentTransformer"/> class.
  /// </summary>
  /// <param name="authorizationUrl">The authorization-code flow authorization endpoint.</param>
  /// <param name="tokenUrl">The authorization-code flow token endpoint.</param>
  /// <param name="scopes">The scopes advertised by the flow, keyed by scope name with a human-readable description.</param>
  /// <exception cref="ArgumentNullException"><paramref name="authorizationUrl"/> or <paramref name="tokenUrl"/> is <see langword="null"/>.</exception>
  public OAuth2SecuritySchemeDocumentTransformer(
    Uri authorizationUrl,
    Uri tokenUrl,
    IReadOnlyDictionary<string, string>? scopes = null)
  {
    ArgumentNullException.ThrowIfNull(authorizationUrl);
    ArgumentNullException.ThrowIfNull(tokenUrl);

    this.authorizationUrl = authorizationUrl;
    this.tokenUrl = tokenUrl;
    this.scopes = scopes ?? new Dictionary<string, string>(StringComparer.Ordinal);
  }

  /// <inheritdoc />
  public Task TransformAsync(
    OpenApiDocument document,
    OpenApiDocumentTransformerContext context,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(document);

    document.Components ??= new OpenApiComponents();
    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
    document.Components.SecuritySchemes.TryAdd(SchemeId, new OpenApiSecurityScheme
    {
      Type = SecuritySchemeType.OAuth2,
      Description = "OAuth2 authorization code flow",
      Flows = new OpenApiOAuthFlows
      {
        AuthorizationCode = new OpenApiOAuthFlow
        {
          AuthorizationUrl = this.authorizationUrl,
          TokenUrl = this.tokenUrl,
          Scopes = new Dictionary<string, string>(this.scopes, StringComparer.Ordinal),
        },
      },
    });

    return Task.CompletedTask;
  }
}
