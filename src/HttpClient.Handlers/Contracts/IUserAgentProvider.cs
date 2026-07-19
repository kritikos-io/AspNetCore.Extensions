namespace Kritikos.HttpClient.Handlers.Contracts;

/// <summary>
/// Defines a contract for generating randomized user agent strings.
/// </summary>
public interface IUserAgentProvider
{
  /// <summary>
  /// Returns a random user agent string.
  /// </summary>
  /// <returns>A user agent as a string.</returns>
  string GetRandomUserAgent();
}
