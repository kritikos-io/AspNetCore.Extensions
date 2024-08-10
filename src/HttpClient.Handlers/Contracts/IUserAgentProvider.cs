namespace Kritikos.HttpClient.Handlers.Contracts;

/// <summary>
/// A basic contract for user agent providers.
/// </summary>
public interface IUserAgentProvider
{
  /// <summary>
  /// Returns a random user agent string.
  /// </summary>
  /// <returns>A user agent as a string.</returns>
  string GetRandomUserAgent();
}
