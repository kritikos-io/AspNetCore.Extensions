namespace Kritikos.AspNetCore.OpenApiExtensions.DocumentTransformers;

using Microsoft.AspNetCore.OpenApi;

using Microsoft.OpenApi;

/// <summary>
/// An OpenAPI document transformer that declares an API-key security scheme carried in a request header, under a
/// configurable key referenced by <see cref="OperationTransformers.AuthorizationCheckOperationTransformer"/>.
/// </summary>
public sealed class ApiKeySecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer
{
  /// <summary>The default security-scheme key (<c>apiKey</c>).</summary>
  public const string SchemeId = "apiKey";

  private readonly string headerName;
  private readonly string schemeId;

  /// <summary>
  /// Initializes a new instance of the <see cref="ApiKeySecuritySchemeDocumentTransformer"/> class.
  /// </summary>
  /// <param name="headerName">The request header carrying the API key (for example, <c>X-API-Key</c>).</param>
  /// <param name="schemeId">The security-scheme key. Defaults to <see cref="SchemeId"/>.</param>
  /// <exception cref="ArgumentException"><paramref name="headerName"/> or <paramref name="schemeId"/> is null or whitespace.</exception>
  public ApiKeySecuritySchemeDocumentTransformer(string headerName, string schemeId = SchemeId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(headerName);
    ArgumentException.ThrowIfNullOrWhiteSpace(schemeId);

    this.headerName = headerName;
    this.schemeId = schemeId;
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
    document.Components.SecuritySchemes.TryAdd(schemeId, new OpenApiSecurityScheme
    {
      Type = SecuritySchemeType.ApiKey,
      In = ParameterLocation.Header,
      Name = headerName,
      Description = "API key",
    });

    return Task.CompletedTask;
  }
}
