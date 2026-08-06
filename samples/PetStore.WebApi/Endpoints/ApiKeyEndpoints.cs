namespace Kritikos.PetStore.WebApi.Endpoints;

using System.Security.Claims;

using Asp.Versioning;

using Kritikos.AspNetCore.MinimalApiExtensions.Authentication;
using Kritikos.AspNetCore.VersioningOptions.Contracts;

using Microsoft.AspNetCore.Http.HttpResults;

public sealed class ApiKeyEndpoints : IVersionedEndpoint
{
  /// <inheritdoc />
  public string Group => "keys";

  /// <inheritdoc />
  public ApiVersion Version => new(1);

  /// <inheritdoc />
  public void MapGroupedEndpoint(RouteGroupBuilder group)
  {
    group.WithTags("Api Keys");

    // Any valid key (reader or admin) can read.
    group.MapGet("read", GetRead)
      .RequireAuthorization(policy => policy
        .AddAuthenticationSchemes(ApiKeyDefaults.AuthenticationScheme)
        .RequireRole("reader", "admin"));

    // Only an admin-scoped key reaches this.
    group.MapGet("admin", GetAdmin)
      .RequireAuthorization(policy => policy
        .AddAuthenticationSchemes(ApiKeyDefaults.AuthenticationScheme)
        .RequireRole("admin"));
  }

  public Ok<string> GetRead(ClaimsPrincipal user)
    => TypedResults.Ok($"{user.Identity?.Name} can read");

  public Ok<string> GetAdmin(ClaimsPrincipal user)
    => TypedResults.Ok($"{user.Identity?.Name} is admin");
}
