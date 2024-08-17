namespace Kritikos.PetStore.WebApi.Endpoints;

using System.ComponentModel.DataAnnotations;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;
using Kritikos.AspNetCore.MinimalApiExtensions.Filters;
using Kritikos.PetStore.WebApi.Controllers;

using Microsoft.AspNetCore.Http.HttpResults;

public class PetEndpoints : IEndpoint
{
  /// <inheritdoc />
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("api/v{apiVersion:apiVersion}");

    var v1 = group.MapGroup("pet")
        .WithApiVersionSet(Program.VersionSet)
        .MapToApiVersion(1)
        .WithTags("Pet")
        .WithOpenApi();

    v1.MapGet("{id:long}", GetPetsV1).AddEndpointFilter<ValidatorFilter>();
    v1
        .AddEndpointFilter<ValidatorFilter>()
        .MapPost("{id:long}", ([Required] long id, CreateFooDto dto) => TypedResults.Ok(dto));

    var v2 = group.MapGroup("pet")
        .WithApiVersionSet(Program.VersionSet)
        .MapToApiVersion(2)
        .WithTags("Pet")
        .WithOpenApi()
        .AddEndpointFilter<ValidatorFilter>();

    v2.MapGet("{id:long}", GetPetsV2);
    v2.MapPost(string.Empty, CreatePetV2);
  }

  public async Task<Ok<PetV1Dto>> GetPetsV1(long id)
  {
    return TypedResults.Ok(new PetV1Dto("Sir Paddington", 3));
  }

  public async Task<Ok<PetV2Dto>> GetPetsV2(long id)
  {
    return TypedResults.Ok(new PetV2Dto("Snuggles", "McFluff", 5));
  }

  public async Task<Ok> CreatePetV2(PetV2Dto dto) => TypedResults.Ok();
}

public record PetV1Dto(string Name, int Age);

public record PetV2Dto(
    [property: Required]
    [property: MinLength(3)]
    string FirstName,
    [property: EmailAddress] string LastName,
    int Age);
