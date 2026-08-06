namespace Kritikos.AspNetCore.OpenApiExtensions.OperationTransformers;

using System.Reflection;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

/// <summary>
/// An OpenAPI operation transformer that adds 401/403 responses and a security requirement to authorized endpoints.
/// Each operation references the OpenAPI security scheme mapped from the authentication scheme its authorization
/// requires; operations without an explicit authentication scheme fall back to <paramref name="securitySchemeId"/>.
/// Operations marked <c>[AllowAnonymous]</c> are left untouched, matching runtime behaviour.
/// </summary>
/// <param name="securitySchemeId">The default security-scheme key for operations that declare no authentication scheme.</param>
/// <param name="schemesByAuthenticationScheme">
/// A map from ASP.NET Core authentication scheme name to the OpenAPI security-scheme key to reference (for example,
/// <c>ApiKey</c> to <c>apiKey</c>). When omitted, every authorized operation uses <paramref name="securitySchemeId"/>.
/// </param>
public sealed class AuthorizationCheckOperationTransformer(
  string securitySchemeId = "bearer",
  IReadOnlyDictionary<string, string>? schemesByAuthenticationScheme = null)
  : IOpenApiOperationTransformer
{
  private readonly IReadOnlyDictionary<string, string> schemesByAuthenticationScheme =
    schemesByAuthenticationScheme ?? new Dictionary<string, string>(StringComparer.Ordinal);

  /// <inheritdoc />
  public Task TransformAsync(
    OpenApiOperation operation,
    OpenApiOperationTransformerContext context,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(operation);
    ArgumentNullException.ThrowIfNull(context);

    var metadata = context.Description.ActionDescriptor.EndpointMetadata;
    operation.OperationId ??= metadata.OfType<MethodInfo>().FirstOrDefault()?.Name;

    if (string.IsNullOrWhiteSpace(operation.OperationId)
        || !OperationSecurity.IsSecured(metadata, out var authorization))
    {
      return Task.CompletedTask;
    }

    OperationSecurity.Apply(
      operation,
      context.Document,
      metadata,
      authorization,
      securitySchemeId,
      this.schemesByAuthenticationScheme);

    return Task.CompletedTask;
  }
}
