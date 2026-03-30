namespace Kritikos.PetStore.WebApi.Endpoints;

using System.ComponentModel.DataAnnotations;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;
using Kritikos.PetStore.WebApi.Models.V1;
using Kritikos.PetStore.WebApi.Models.V2;

using Microsoft.AspNetCore.Http.HttpResults;

public sealed class PetEndpoints : IEndpoint
{
  /// <inheritdoc />
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("api/v{apiVersion:apiVersion}");

    var v1 = group.MapGroup("pet")
      .WithApiVersionSet(Program.VersionSet)
      .MapToApiVersion(1)
      .WithTags("Pet");

    v1.MapGet("{id:long}", GetPetsV1);
    v1.MapPost("{id:long}", static ([Required] long id, CreateFooDto dto) => TypedResults.Ok(dto));

    var v2 = group.MapGroup("pet")
      .WithApiVersionSet(Program.VersionSet)
      .MapToApiVersion(2)
      .WithTags("Pet");

    v2.MapGet("{id:long}", GetPetsV2);
    v2.MapPost(string.Empty, CreatePetV2);
  }

  public Ok<PetV1Dto> GetPetsV1(long id)
    => TypedResults.Ok(new PetV1Dto("Sir Paddington", 3));

  public Ok<PetV2Dto> GetPetsV2(long id)
    => TypedResults.Ok(new PetV2Dto("Snuggles", "McFluff", 5));

  public Ok CreatePetV2(PetV2Dto dto) => TypedResults.Ok();
}
