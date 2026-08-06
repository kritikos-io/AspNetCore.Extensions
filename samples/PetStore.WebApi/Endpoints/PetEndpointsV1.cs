namespace Kritikos.PetStore.WebApi.Endpoints;

using System.ComponentModel.DataAnnotations;

using Asp.Versioning;

using Kritikos.AspNetCore.VersioningOptions.Contracts;
using Kritikos.PetStore.WebApi.Models.V1;

using Microsoft.AspNetCore.Http.HttpResults;

public sealed class PetEndpointsV1 : IVersionedEndpoint
{
  /// <inheritdoc />
  public string Group => "pet";

  /// <inheritdoc />
  public ApiVersion Version => new(1);

  /// <inheritdoc />
  public void MapGroupedEndpoint(RouteGroupBuilder group)
  {
    group.WithTags("Pet");

    group.MapGet("{id:long}", GetPetsV1);
    group.MapPost("{id:long}", static ([Required] long id, CreateFooDto dto) => TypedResults.Ok(dto));

    // HTTP QUERY: a safe, idempotent search whose criteria travel in the request body (RFC 10008).
    group.MapMethods(string.Empty, ["QUERY"], SearchPetsV1);
  }

  public Ok<PetV1Dto> GetPetsV1(long id)
    => TypedResults.Ok(new PetV1Dto("Sir Paddington", 3));

  public Ok<PetV1Dto[]> SearchPetsV1(PetSearchV1Dto query)
    => TypedResults.Ok<PetV1Dto[]>([new(query.NameContains ?? "Sir Paddington", query.MinAge ?? 3)]);
}
