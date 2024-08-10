namespace Kritikos.PetStore.WebApi.Controllers;

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
