namespace Kritikos.PetStore.WebApi;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;
using Kritikos.AspNetCore.OpenApiVersioningOptions.Options;
using Kritikos.Extensions.Options.Contracts;

public class MyOpenApiInfoOptions : OpenApiInfoOptions, IOptionsDefinition
{
  /// <inheritdoc />
  public static string Location { get; } = "OpenApi";
}
