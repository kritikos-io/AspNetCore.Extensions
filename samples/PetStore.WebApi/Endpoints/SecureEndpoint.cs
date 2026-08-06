namespace Kritikos.PetStore.WebApi.Endpoints;

using System.Security.Claims;

using Asp.Versioning;

using Kritikos.AspNetCore.VersioningOptions.Contracts;

using Microsoft.AspNetCore.Http.HttpResults;

public sealed class SecureEndpoint : IVersionedEndpoint
{
  /// <inheritdoc />
  public string Group => "secure";

  /// <inheritdoc />
  public ApiVersion Version => new(1);

  /// <inheritdoc />
  public void MapGroupedEndpoint(RouteGroupBuilder group)
  {
    group.WithTags("Secure");

    group.MapGet("ping", Ping).RequireAuthorization();
  }

  public Ok<string> Ping(ClaimsPrincipal user)
    => TypedResults.Ok(user.Identity?.Name ?? "authenticated");
}
