namespace Kritikos.PetStore.WebApi.Endpoints;

using Kritikos.AspNetCore.FeatureManagementOptions;
using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.FeatureManagement;

public class FeatureGatedEndpoints : IEndpoint
{
  /// <inheritdoc />
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/feature")
        .WithOpenApi();

    group.MapGet("single", () => TypedResults.Ok("on"))
        .WithFeatureFlags("MyFeature");

    group.MapGet("or", () => TypedResults.Ok("on"))
        .WithFeatureFlags(RequirementType.Any, "FirstOrFlag", "SecondOrFlag");

    group.MapGet("and", () => TypedResults.Ok("on"))
        .WithFeatureFlags(RequirementType.All, "FirstAndFlag", "SecondAndFlag");
  }
}
