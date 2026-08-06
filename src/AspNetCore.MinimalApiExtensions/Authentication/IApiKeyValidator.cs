namespace Kritikos.AspNetCore.MinimalApiExtensions.Authentication;

using System.Security.Claims;

/// <summary>
/// Validates a presented API key and resolves the identity it represents. Implementations are provided by the
/// consuming application (for example, backed by configuration, a database, or a secret store).
/// </summary>
public interface IApiKeyValidator
{
  /// <summary>
  /// Validates <paramref name="apiKey"/> and returns the authenticated <see cref="ClaimsPrincipal"/>, or
  /// <see langword="null"/> when the key is not valid.
  /// </summary>
  /// <param name="apiKey">The API key presented on the request.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>The principal for a valid key; otherwise <see langword="null"/>.</returns>
  ValueTask<ClaimsPrincipal?> ValidateAsync(string apiKey, CancellationToken cancellationToken);
}
