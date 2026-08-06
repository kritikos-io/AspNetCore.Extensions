namespace Kritikos.AspNetCore.OpenApiExtensions.DocumentTransformers;

using Microsoft.AspNetCore.OpenApi;

using Microsoft.OpenApi;

/// <summary>
/// An OpenAPI document transformer that declares the HTTP bearer (JWT) security scheme the API validates,
/// under the key <c>bearer</c> referenced by <see cref="OperationTransformers.AuthorizationCheckOperationTransformer"/>.
/// </summary>
public sealed class BearerSecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer
{
  /// <inheritdoc />
  public Task TransformAsync(
    OpenApiDocument document,
    OpenApiDocumentTransformerContext context,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(document);

    document.Components ??= new OpenApiComponents();
    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
    document.Components.SecuritySchemes.TryAdd("bearer", new OpenApiSecurityScheme
    {
      Type = SecuritySchemeType.Http,
      Scheme = "bearer",
      BearerFormat = "JWT",
      Description = "JWT bearer token",
    });

    return Task.CompletedTask;
  }
}
