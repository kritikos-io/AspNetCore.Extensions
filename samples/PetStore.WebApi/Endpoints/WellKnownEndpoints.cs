namespace Kritikos.PetStore.WebApi.Endpoints;

using Asp.Versioning.Builder;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;
using Kritikos.SemanticVersioning;

using Microsoft.AspNetCore.Http.HttpResults;

public sealed class WellKnownEndpoints : IEndpoint
{
  /// <inheritdoc />
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var versionSet = app.ServiceProvider.GetRequiredService<ApiVersionSet>();

    app.MapGroup(".well-known")
      .WithApiVersionSet(versionSet)
      .MapGet("version", GetSemanticVersion)
      .IsApiVersionNeutral();
  }

  public Ok<string> GetSemanticVersion(SemanticVersionDescriptor version)
    => TypedResults.Ok(version.ToString());
}
