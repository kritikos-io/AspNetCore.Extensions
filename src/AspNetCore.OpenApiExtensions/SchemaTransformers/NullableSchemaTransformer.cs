namespace Kritikos.AspNetCore.OpenApiExtensions.SchemaTransformers;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

public class NullableSchemaTransformer : IOpenApiSchemaTransformer
{
  /// <inheritdoc />
  public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(schema);

    if (schema.Properties is null)
    {
      return Task.CompletedTask;
    }

    foreach (var property in schema.Properties)
    {
      if (schema.Required?.Contains(property.Key) != true)
      {
        property.Value.Nullable = false;
      }
    }

    return Task.CompletedTask;
  }
}
