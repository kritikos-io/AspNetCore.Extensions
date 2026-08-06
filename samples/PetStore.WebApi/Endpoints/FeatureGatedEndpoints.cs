namespace Kritikos.PetStore.WebApi.Endpoints;

using Asp.Versioning;

using Kritikos.AspNetCore.FeatureManagementOptions;
using Kritikos.AspNetCore.VersioningOptions.Contracts;

using Microsoft.FeatureManagement;

public sealed class FeatureGatedEndpoints : IVersionedEndpoint
{
  /// <inheritdoc />
  public string Group => "feature";

  /// <inheritdoc />
  public ApiVersion Version => new(2);

  /// <inheritdoc />
  public void MapGroupedEndpoint(RouteGroupBuilder group)
  {
    group.WithTags("Features");

    group.MapGet("single", static () => TypedResults.Ok("on"))
        .WithFeatureFlags("MyFeature");

    group.MapGet("or", static () => TypedResults.Ok("on"))
        .WithFeatureFlags(RequirementType.Any, "FirstOrFlag", "SecondOrFlag");

    group.MapGet("and", static () => TypedResults.Ok("on"))
        .WithFeatureFlags(RequirementType.All, "FirstAndFlag", "SecondAndFlag");
  }
}
