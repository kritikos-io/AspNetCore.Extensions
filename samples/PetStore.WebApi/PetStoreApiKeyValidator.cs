namespace Kritikos.PetStore.WebApi;

using System.Security.Claims;

using Kritikos.AspNetCore.MinimalApiExtensions.Authentication;

// Demo in-memory key store mapping each key to an owner and roles; a real app would use a hashed secret store.
public sealed class PetStoreApiKeyValidator : IApiKeyValidator
{
  private static readonly Dictionary<string, (string Owner, string[] Roles)> Keys = new(StringComparer.Ordinal)
  {
    ["reader-key-123"] = ("reader-client", ["reader"]),
    ["admin-key-456"] = ("admin-client", ["reader", "admin"]),
  };

  /// <inheritdoc />
  public ValueTask<ClaimsPrincipal?> ValidateAsync(string apiKey, CancellationToken cancellationToken)
  {
    if (!Keys.TryGetValue(apiKey, out var entry))
    {
      return ValueTask.FromResult<ClaimsPrincipal?>(null);
    }

    var claims = new List<Claim> { new(ClaimTypes.Name, entry.Owner) };
    claims.AddRange(entry.Roles.Select(static role => new Claim(ClaimTypes.Role, role)));

    // The identity MUST carry an authentication type, otherwise IsAuthenticated is false and authorization rejects it.
    return ValueTask.FromResult<ClaimsPrincipal?>(
      new ClaimsPrincipal(new ClaimsIdentity(claims, ApiKeyDefaults.AuthenticationScheme)));
  }
}
