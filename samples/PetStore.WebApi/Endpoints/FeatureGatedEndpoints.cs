namespace Kritikos.PetStore.WebApi.Endpoints;

using Kritikos.AspNetCore.FeatureManagementOptions;
using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.FeatureManagement;

public sealed class FeatureGatedEndpoints : IEndpoint
{
  /// <inheritdoc />
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/v{version:apiVersion}/feature")
        .WithOpenApi()
        .WithApiVersionSet(Program.VersionSet)
        .MapToApiVersion(2)
        .WithTags("Features");

    group.MapGet("single", static () => TypedResults.Ok("on"))
        .WithFeatureFlags("MyFeature");

    group.MapGet("or", static () => TypedResults.Ok("on"))
        .WithFeatureFlags(RequirementType.Any, "FirstOrFlag", "SecondOrFlag");

    group.MapGet("and", static () => TypedResults.Ok("on"))
        .WithFeatureFlags(RequirementType.All, "FirstAndFlag", "SecondAndFlag");
  }
}
