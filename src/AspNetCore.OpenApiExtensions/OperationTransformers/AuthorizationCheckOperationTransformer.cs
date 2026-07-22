namespace Kritikos.AspNetCore.OpenApiExtensions.OperationTransformers;

using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

/// <summary>
/// An OpenAPI operation transformer that adds 401/403 responses and an OAuth security requirement to authorized endpoints.
/// </summary>
public sealed class AuthorizationCheckOperationTransformer : IOpenApiOperationTransformer
{
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
    if (!metadata.OfType<IAuthorizeData>().Any() || string.IsNullOrWhiteSpace(operation.OperationId))
    {
      return Task.CompletedTask;
    }

    operation.Responses ??= [];
    operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
    operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

    var oAuthScheme = new OpenApiSecuritySchemeReference("openid");

    operation.Security = [new() { [oAuthScheme] = ["openid"] }];

    return Task.CompletedTask;
  }
}
