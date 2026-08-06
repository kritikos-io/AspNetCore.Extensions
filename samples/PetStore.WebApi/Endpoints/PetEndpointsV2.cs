namespace Kritikos.PetStore.WebApi.Endpoints;

using Asp.Versioning;

using Kritikos.AspNetCore.VersioningOptions.Contracts;
using Kritikos.PetStore.WebApi.Models.V2;

using Microsoft.AspNetCore.Http.HttpResults;

public sealed class PetEndpointsV2 : IVersionedEndpoint
{
  /// <inheritdoc />
  public string Group => "pet";

  /// <inheritdoc />
  public ApiVersion Version => new(2);

  /// <inheritdoc />
  public void MapGroupedEndpoint(RouteGroupBuilder group)
  {
    group.WithTags("Pet");

    group.MapGet("{id:long}", GetPetsV2);
    group.MapPost(string.Empty, CreatePetV2);
  }

  public Ok<PetV2Dto> GetPetsV2(long id)
    => TypedResults.Ok(new PetV2Dto("Snuggles", "McFluff", 5));

  public Ok CreatePetV2(PetV2Dto dto) => TypedResults.Ok();
}
