namespace Kritikos.PetStore.WebApi.Controllers;

using System.ComponentModel.DataAnnotations;

using Asp.Versioning;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v{apiVersion:apiVersion}/[controller]")]
[ApiVersion(3)]
public class FooController
{
  [HttpGet("")]
  public async Task<Ok> GetFoo()
  {
    return TypedResults.Ok();
  }
}

public record CreateFooDto(
    [property: Required]
    [property: MinLength(3)]
    [property: EmailAddress]
    string Name,
    [property: MinLength(8)]
    [property: Required]
    [property: Phone]
    string Phone,
    [property: Required]
    [property: AllowedValues(1, 2, 3, 4)]
    int Age);
