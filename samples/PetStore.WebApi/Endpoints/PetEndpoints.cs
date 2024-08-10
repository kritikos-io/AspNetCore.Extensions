namespace Kritikos.PetStore.WebApi.Endpoints;

using Asp.Versioning;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

using Microsoft.AspNetCore.Http.HttpResults;

public class PetEndpoints : IEndpoint
{
  /// <inheritdoc />
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("api/v{apiVersion:apiVersion}");

    group.MapGroup("pet")
        .WithApiVersionSet(Program.VersionSet)
        .MapToApiVersion(1)
        .WithTags("Pet")
        .WithOpenApi()
        .MapGet("{id:long}", GetPetsV1);

    group.MapGroup("pet")
        .WithApiVersionSet(Program.VersionSet)
        .MapToApiVersion(2)
        .WithTags("Pet")
        .WithOpenApi()
        .MapGet("{id:long}", GetPetsV2);
  }

  public async Task<Ok<PetV1Dto>> GetPetsV1(long id)
  {
    return TypedResults.Ok(new PetV1Dto("Sir Paddington", 3));
  }

  public async Task<Ok<PetV2Dto>> GetPetsV2(long id)
  {
    return TypedResults.Ok(new PetV2Dto("Snuggles", "McFluff", 5));
  }
}

public record PetV1Dto(string Name, int Age);

public record PetV2Dto(string FirstName, string LastName, int Age);
